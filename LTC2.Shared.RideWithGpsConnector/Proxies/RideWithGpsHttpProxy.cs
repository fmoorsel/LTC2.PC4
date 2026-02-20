using LTC2.Shared.Http.Exceptions;
using LTC2.Shared.Http.Proxies;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.RideWithGpsConnector.Interfaces;
using LTC2.Shared.RideWithGpsConnector.Models.Requests;
using LTC2.Shared.RideWithGpsConnector.Models.Responses;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LTC2.Shared.RideWithGpsConnector.Proxies
{
    public class RideWithGpsHttpProxy : AbstractHttpProxy, IRideWithGpsHttpProxy
    {
        private readonly ILogger<RideWithGpsHttpProxy> _logger;
        private readonly RideWithGpsHttpProxySettings _settings;
        private readonly GenericSettings _genericSettings;

        public RideWithGpsHttpProxy(
            ILogger<RideWithGpsHttpProxy> logger,
            RideWithGpsHttpProxySettings settings,
            GenericSettings genericSettings) : base(logger, settings)
        {
            _logger = logger;
            _settings = settings;
            _genericSettings = genericSettings;
        }

        public async Task<AuthorizeResponse> GetToken(AuthorizeRequest request)
        {
            var parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>("client_id", _settings.ClientId));
            parameters.Add(new KeyValuePair<string, string>("client_secret", _settings.ClientSecret));
            parameters.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
            parameters.Add(new KeyValuePair<string, string>("code", request.Code));
            parameters.Add(new KeyValuePair<string, string>("redirect_uri", request.RedirectUri));
            return await ExecuteFormUrlEncodedRequest<AuthorizeResponse>("/oauth/token", null, parameters);
        }

        public async Task<CurrentUserResponse> GetCurrentUser(string accessToken)
        {
            var authHeader = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            return await ExecuteGetRequest<CurrentUserResponse>("/api/v1/users/current", authHeader);
        }

        public async Task<List<RwGpsSyncItem>> GetActivities(GetActivitiesRequest request, string accessToken)
        {
            var since = Uri.EscapeDataString(request.After.ToString("o"));
            var uri = $"/api/v1/sync.json?since={since}&assets=trip";
            var authHeader = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var response = await ExecuteGetRequest<RwGpsSyncResponse>(uri, authHeader);

            return response?.Items
                ?.Where(t => t.Action == "created" || t.Action == "updated")
                .ToList() ?? new List<RwGpsSyncItem>();
        }

        public async Task<RwGpsTrip> GetTrip(long id, bool bypassCache, string accessToken)
        {
            var cacheFolder = Path.Combine(_genericSettings.CacheFolder, "Trips");
            var fileName = Path.Combine(cacheFolder, $"r{id}");

            if (!Directory.Exists(cacheFolder))
            {
                Directory.CreateDirectory(cacheFolder);
            }

            if (!bypassCache && File.Exists(fileName))
            {
                try
                {
                    var json = File.ReadAllText(fileName);
                    return JsonConvert.DeserializeObject<RwGpsTrip>(json);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Unable to read trip {id} from cache {fileName} due to {ex.Message}");
                }
            }

            var authHeader = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var retryCount = 0;

            while (retryCount < 3)
            {
                try
                {
                    var response = await ExecuteGetRequest<RwGpsTripResponse>($"/api/v1/trips/{id}.json", authHeader);

                    var trip = response?.Trip;
                    if (trip == null)
                        return null;

                    var result = new RwGpsTrip
                    {
                        Name = trip.Name,
                        ActivityType = trip.Activity_type,
                        Distance = trip.Distance,
                        StartTime = trip.Departed_at,
                        MovingTime = trip.Moving_time,
                        Coordinates = trip.Track_points
                            ?.Select(p => new List<double> { p.Y, p.X })
                            .ToList() ?? new List<List<double>>()
                    };

                    try
                    {
                        File.WriteAllText(fileName, JsonConvert.SerializeObject(result));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Unable to cache trip {id} in {fileName} due to {ex.Message}");
                    }

                    return result;
                }
                catch (HttpProxyException hpe)
                {
                    if (hpe.Code < (int)HttpStatusCode.InternalServerError)
                    {
                        throw;
                    }

                    retryCount++;

                    if (retryCount >= 3)
                    {
                        throw;
                    }
                }
            }

            return null;
        }
    }
}
