using LTC2.Shared.Http.Exceptions;
using LTC2.Shared.Http.Proxies;
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
using System.Net;
using System.Net.Http.Headers;
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

            parameters.Add(clientIdParameter);
            parameters.Add(clientSecretParameter);
            parameters.Add(codeParameter);
            parameters.Add(grantTypeParameter);

            return await ExecuteFormUrlEncodedRequest<AuthorizeResponse>("/oauth/token", null, parameters);
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
                            var responseHeaders = hpe.Headers;

                            SanitizeUsageAndLimitHeaders(responseHeaders);

                            var limitsOnly = new LimitsOnlyResponse(responseHeaders[_stravaRateLimit], responseHeaders[_stravaRateUsage]);

                            _logger.LogWarning($"Too many Strava requests [{limitsOnly.QuarterRateLimit},{limitsOnly.DayRateLimit}] [{limitsOnly.QuarterRateUsage},{limitsOnly.DayRateUsage}]");

                            throw new StraveTooManyRequestsException(limitsOnly, hpe);
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
                            var responseHeaders = hpe.Headers;

                            SanitizeUsageAndLimitHeaders(responseHeaders);

                            var limitsOnly = new LimitsOnlyResponse(responseHeaders[_stravaRateLimit], responseHeaders[_stravaRateUsage]);

                            _logger.LogWarning($"Too many Strava requests [{limitsOnly.QuarterRateLimit},{limitsOnly.DayRateLimit}] [{limitsOnly.QuarterRateUsage},{limitsOnly.DayRateUsage}]");

                            throw new StraveTooManyRequestsException(limitsOnly, hpe);
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
