using LTC2.Shared.Http.Exceptions;
using LTC2.Shared.Http.Proxies;
using LTC2.Shared.Models.Domain;
using LTC2.Shared.Models.Requests;
using LTC2.Shared.Models.Responses;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.StravaConnector.Exceptions;
using LTC2.Shared.StravaConnector.Interfaces;
using LTC2.Shared.StravaConnector.Models;
using LTC2.Shared.StravaConnector.Models.Requests;
using LTC2.Shared.StravaConnector.Models.Responses;
using LTC2.Shared.Utils.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace LTC2.Shared.StravaConnector.Proxies
{
    public class StravaHttpProxy : AbstractHttpProxy, IStravaHttpProxy
    {
        private readonly ILogger<StravaHttpProxy> _logger;
        private readonly StravaHttpProxySettings _stravaHttpProxySettings;
        private readonly GenericSettings _genericSettings;

        private readonly object _lockObject = new object();

        public static int StravaTooManyRequestsResponseCode = 429;

        private readonly string _stravaRateLimit = "X-RateLimit-Limit";
        private readonly string _stravaRateUsage = "X-RateLimit-Usage";

        private readonly string _stravaReadRateLimit = "X-ReadRateLimit-Limit";
        private readonly string _stravaReadRateUsage = "X-ReadRateLimit-Usage";

        public StravaHttpProxy(
            ILogger<StravaHttpProxy> logger,
            StravaHttpProxySettings stravaHttpProxySettings,
            GenericSettings genericSettings) : base(logger, stravaHttpProxySettings)
        {
            _logger = logger;
            _stravaHttpProxySettings = stravaHttpProxySettings;
            _genericSettings = genericSettings;

            _logger.LogInformation($"Using Strava with clientid {_stravaHttpProxySettings.ClientId}");
        }

        public async Task<AuthorizeResponse> GetToken(AuthorizeRequest request)
        {
            var parameters = new List<KeyValuePair<string, string>>();

            var clientIdParameter = new KeyValuePair<string, string>("client_id", _stravaHttpProxySettings.ClientId);
            var clientSecretParameter = new KeyValuePair<string, string>("client_secret", _stravaHttpProxySettings.ClientSecret);
            var grantTypeParameter = new KeyValuePair<string, string>("grant_type", request.Type == AuthorizeType.RefreshToken ? "refresh_token" : "authorization_code");
            var codeParameter = new KeyValuePair<string, string>(request.Type == AuthorizeType.RefreshToken ? "refresh_token" : "code", request.Code);

            _logger.LogInformation($"Using Strava with clientid {clientIdParameter}");

            parameters.Add(clientIdParameter);
            parameters.Add(clientSecretParameter);
            parameters.Add(codeParameter);
            parameters.Add(grantTypeParameter);

            var count = 0;
            Exception exception = null;
            while (count < 3)
            {
                count++;

                try
                {
                    return await ExecuteFormUrlEncodedRequest<AuthorizeResponse>("/oauth/token", null, parameters);
                }
                catch (Exception ex)
                {
                    Thread.Sleep(500);

                    exception = ex;
                }
            }

            throw exception;
        }

        public async Task<GetActivitiesResponse> GetActivities(GetActivitiesRequest request, string accessToken)
        {
            var perPage = $"per_page={request.PerPage}";
            var page = $"&page={request.Page}";
            var before = request.Before.HasValue ? $"&before={DateConverter.GetSecondsSinceUnixEpoch(request.Before.Value)}" : "";
            var after = request.After.HasValue ? $"&after={DateConverter.GetSecondsSinceUnixEpoch(request.After.Value)}" : "";

            var retryCount = 0;

            while (retryCount < 3)
            {
                try
                {
                    var authHeader = new AuthenticationHeaderValue("Bearer", accessToken);

                    var responseHeaders = CreateRatesHeaderFilter();

                    var activities = await ExecuteGetRequest<List<StravaActivity>>($"/api/v3/athlete/activities?{perPage}{page}{before}{after}", responseHeaders, authHeader);

                    SanitizeUsageAndLimitHeaders(responseHeaders);

                    var result = new GetActivitiesResponse(responseHeaders[_stravaRateLimit], responseHeaders[_stravaRateUsage])
                    {
                        Activities = activities
                    };

                    return result;
                }
                catch (HttpProxyException hpe)
                {
                    if (hpe.Code == StravaTooManyRequestsResponseCode)
                    {
                        if (hpe.Headers != null)
                        {
                            HandleTooManyRequests(hpe);
                        }

                        throw;
                    }
                    else if (hpe.Code < (int)HttpStatusCode.InternalServerError)
                    {
                        throw;
                    }
                    else
                    {
                        retryCount++;

                        if (retryCount >= 3)
                        {
                            throw;
                        }
                    }
                }
            }

            return null;
        }

        public async Task<GetActivityCoordinateStreamResponse> GetActivityCoordinateStream(GetActivityCoordinateStreamRequest request, string accessToken)
        {
            var cacheFolder = Path.Combine(_genericSettings.CacheFolder, "Streams");
            var fileName = Path.Combine(cacheFolder, $"r{request.ActivityId}");

            if (!Directory.Exists(cacheFolder))
            {
                Directory.CreateDirectory(cacheFolder);
            }

            var cachedResult = GetStreamFromCaches(request);

            if (cachedResult != null)
            {
                return cachedResult;
            }

            var retryCount = 0;

            while (retryCount < 3)
            {
                try
                {
                    var authHeader = new AuthenticationHeaderValue("Bearer", accessToken);

                    var responseHeaders = CreateRatesHeaderFilter();

                    var details = await ExecuteGetRequest<StravaActivityCoordinateStream>($"/api/v3/activities/{request.ActivityId}/streams?keys=latlng&key_by_type=true", responseHeaders, authHeader);

                    SanitizeUsageAndLimitHeaders(responseHeaders);

                    var result = new GetActivityCoordinateStreamResponse(responseHeaders[_stravaRateLimit], responseHeaders[_stravaRateUsage])
                    {
                        ActivityCoordinateStream = details
                    };

                    try
                    {
                        var json = JsonConvert.SerializeObject(details);
                        File.WriteAllText(fileName, json);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Unable to cache activity {request.ActivityId} in {fileName} due to {ex.Message}");
                    }

                    return result;
                }
                catch (HttpProxyException hpe)
                {
                    if (hpe.Code == StravaTooManyRequestsResponseCode)
                    {
                        if (hpe.Headers != null)
                        {
                            HandleTooManyRequests(hpe);
                        }

                        throw;
                    }
                    else if (hpe.Code < (int)HttpStatusCode.InternalServerError)
                    {
                        throw;
                    }
                    else
                    {
                        retryCount++;

                        if (retryCount >= 3)
                        {
                            throw;
                        }
                    }
                }
            }

            return null;
        }

        public async Task<GetRoutesResponse> GetRoutes(GetRoutesRequest request, string accessToken)
        {
            var perPage = $"per_page={_stravaHttpProxySettings.MaxRoutesCount}";

            var retryCount = 0;

            while (retryCount < 3)
            {
                try
                {
                    var authHeader = new AuthenticationHeaderValue("Bearer", accessToken);

                    var responseHeaders = CreateRatesHeaderFilter();

                    var stravaRoutes = await ExecuteGetRequest<List<StravaRoute>>($"/api/v3/athletes/{request.AthleteId}/routes?{perPage}", responseHeaders, authHeader);

                    SanitizeUsageAndLimitHeaders(responseHeaders);

                    var sourceRoutes = stravaRoutes.Select(r => new SourceRoute
                    {
                        RouteId = r.RouteId,
                        Name = r.Name.Length >= 40 ? r.Name.Substring(0, 40) + "..." : r.Name,
                        Timestamp = r.Timestamp,
                        Distance = r.Distance
                    }).ToList();

                    var result = new GetRoutesResponse
                    {
                        Routes = sourceRoutes
                    };

                    ApplyRateLimits(result, responseHeaders[_stravaRateLimit], responseHeaders[_stravaRateUsage]);

                    return result;
                }
                catch (HttpProxyException hpe)
                {
                    if (hpe.Code == StravaTooManyRequestsResponseCode)
                    {
                        if (hpe.Headers != null)
                        {
                            HandleTooManyRequests(hpe);
                        }

                        throw;
                    }
                    else if (hpe.Code < (int)HttpStatusCode.InternalServerError)
                    {
                        throw;
                    }
                    else
                    {
                        retryCount++;

                        if (retryCount >= 3)
                        {
                            throw;
                        }
                    }
                }
            }

            return null;
        }

        public async Task<GetRouteDetailsAsGpxReponse> GetRouteAsGpx(GetRouteDetailsAsGpxRequest request, string accessToken)
        {
            var retryCount = 0;

            while (retryCount < 3)
            {
                try
                {
                    var authHeader = new AuthenticationHeaderValue("Bearer", accessToken);

                    var responseHeaders = CreateRatesHeaderFilter();

                    var gpx = await ExecuteGetRequest($"/api/v3/routes/{request.RouteId}/export_gpx", authHeader, responseHeaders);

                    SanitizeUsageAndLimitHeaders(responseHeaders);

                    var result = new GetRouteDetailsAsGpxReponse
                    {
                        Gpx = gpx
                    };

                    ApplyRateLimits(result, responseHeaders[_stravaRateLimit], responseHeaders[_stravaRateUsage]);

                    return result;
                }
                catch (HttpProxyException hpe)
                {
                    if (hpe.Code == StravaTooManyRequestsResponseCode)
                    {
                        if (hpe.Headers != null)
                        {
                            HandleTooManyRequests(hpe);
                        }

                        throw;
                    }
                    else if (hpe.Code < (int)HttpStatusCode.InternalServerError)
                    {
                        throw;
                    }
                    else
                    {
                        retryCount++;

                        if (retryCount >= 3)
                        {
                            throw;
                        }
                    }
                }
            }

            return null;

        }

        private void HandleTooManyRequests(HttpProxyException hpe)
        {
            var responseHeaders = hpe.Headers;

            SanitizeUsageAndLimitHeaders(responseHeaders);

            var limitsOnly = new LimitsOnlyResponse(responseHeaders[_stravaRateLimit], responseHeaders[_stravaRateUsage]);

            _logger.LogWarning($"Too many Strava requests [{limitsOnly.QuarterRateLimit},{limitsOnly.DayRateLimit}] [{limitsOnly.QuarterRateUsage},{limitsOnly.DayRateUsage}]");

            throw new StraveTooManyRequestsException(limitsOnly, hpe);

        }

        private GetActivityCoordinateStreamResponse GetStreamFromCaches(GetActivityCoordinateStreamRequest request)
        {
            if (!request.BypassCache)
            {
                var result = GetStreamFromCache(request, false);

                if (result != null)
                {
                    return result;
                }

                return GetStreamFromCache(request, true);
            }

            return null;
        }

        private GetActivityCoordinateStreamResponse GetStreamFromCache(GetActivityCoordinateStreamRequest request, bool archiveCache)
        {
            var cacheFolder = Path.Combine(_genericSettings.CacheFolder, "Streams");

            if (archiveCache)
            {
                cacheFolder = Path.Combine(cacheFolder, $"{request.AthleteId}");
            }

            var fileName = Path.Combine(cacheFolder, $"r{request.ActivityId}");

            if (!request.BypassCache && File.Exists(fileName))
            {
                try
                {
                    var json = File.ReadAllText(fileName);
                    var cachedResult = JsonConvert.DeserializeObject<StravaActivityCoordinateStream>(json);

                    var result = new GetActivityCoordinateStreamResponse()
                    {
                        ActivityCoordinateStream = cachedResult
                    };

                    return result;
                }
                catch (Exception ex)
                {
                    var archiveToken = archiveCache ? "archive" : "";

                    _logger.LogWarning(ex, $"Unable to read activity {request.ActivityId} in {fileName} from {archiveToken}cache due to {ex.Message}");
                }
            }

            return null;
        }

        private void SanitizeUsageAndLimitHeaders(Dictionary<string, string> headers)
        {
            // read limit is always to lowest but not used in legacy api's, only new api's
            if (headers.ContainsKey(_stravaReadRateLimit) &&
                headers[_stravaReadRateLimit] != null &&
                headers.ContainsKey(_stravaReadRateUsage) &&
                headers[_stravaReadRateUsage] != null)
            {
                headers[_stravaRateLimit] = headers[_stravaReadRateLimit];
                headers[_stravaRateUsage] = headers[_stravaReadRateUsage];
            }
        }

        private void ApplyRateLimits(ConnectorResponse response, string limits, string usage)
        {
            try
            {
                if (limits != null)
                {
                    limits = limits.Trim();
                    var rateLimits = limits.Split(',');
                    response.QuarterRateLimit = int.Parse(rateLimits[0]);
                    response.DayRateLimit = int.Parse(rateLimits[1]);
                }

                if (usage != null)
                {
                    usage = usage.Trim();
                    var rateUsage = usage.Split(',');
                    response.QuarterRateUsage = int.Parse(rateUsage[0]);
                    response.DayRateUsage = int.Parse(rateUsage[1]);
                }

                response.HasLimits = (limits != null) && (usage != null);
            }
            catch (Exception)
            {
            }
        }

        private Dictionary<string, string> CreateRatesHeaderFilter()
        {
            var responseHeaders = new Dictionary<string, string>()
                    {
                        { _stravaRateLimit, null },
                        { _stravaRateUsage, null },
                        { _stravaReadRateLimit, null },
                        { _stravaReadRateUsage, null }
                    };

            return responseHeaders;
        }


    }
}
