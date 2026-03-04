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
using System.Xml.Linq;

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
            var uri = $"/api/v1/sync.json?since={since}&assets=trips";
            var authHeader = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var retryCount = 0;

            while (retryCount < 3)
            {
                try
                {
                    var response = await ExecuteGetRequest<RwGpsSyncResponse>(uri, authHeader);

                    var deletedItems = response?.Items?.Where(t => t.Action == "deleted");
                    var updatedItems = response?.Items?.Where(t => t.Action == "updated" && !deletedItems.Any(d => d.Item_id == t.Item_id));
                    var createdItems = response?.Items?.Where(t => t.Action == "created" && !deletedItems.Any(d => d.Item_id == t.Item_id) && !updatedItems.Any(d => d.Item_id == t.Item_id));

                    var resultItems = updatedItems?.Concat(createdItems);

                    return resultItems.OrderBy(i => i.Datetime).ToList() ?? new List<RwGpsSyncItem>();
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

            return new List<RwGpsSyncItem>();
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

                    var coordinates = trip.Track_points?
                        .Select(p => new List<double> { p.X, p.Y })
                        .ToList() ?? new List<List<double>>();

                    coordinates = SanitizeTrack(coordinates);

                    var result = new RwGpsTrip
                    {
                        Name = trip.Name,
                        ActivityType = trip.Activity_type,
                        Distance = trip.Distance,
                        StartTime = trip.Departed_at,
                        MovingTime = trip.Moving_time,
                        Duration = trip.Duration,
                        Stationary = trip.Stationary,
                        Coordinates = coordinates
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

        public async Task<string> GetRouteAsGpx(long id, string accessToken)
        {
            var authHeader = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var retryCount = 0;

            while (retryCount < 3)
            {
                try
                {
                    var response = await ExecuteGetRequest<RwGpsRouteResponse>($"/api/v1/routes/{id}.json", authHeader);

                    var route = response?.Route;
                    if (route == null)
                    {
                        return null;
                    }

                    var trackPoints = route.Track_points ?? new List<RwGpsRouteTrackPoint>();

                    XNamespace gpxNs = "http://www.topografix.com/GPX/1/1";

                    var doc = new XDocument(
                        new XDeclaration("1.0", "UTF-8", null),
                        new XElement(gpxNs + "gpx",
                            new XAttribute("version", "1.1"),
                            new XAttribute("creator", "RideWithGPS"),
                            new XElement(gpxNs + "trk",
                                new XElement(gpxNs + "name", route.Name),
                                new XElement(gpxNs + "trkseg",
                                    trackPoints.Select(point =>
                                        new XElement(gpxNs + "trkpt",
                                            new XAttribute("lat", point.Y),
                                            new XAttribute("lon", point.X),
                                            new XElement(gpxNs + "ele", point.E)
                                        )
                                    )
                                )
                            )
                        )
                    );

                    return doc.Declaration + Environment.NewLine + doc.ToString();
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

        public async Task<List<RwGpsRoute>> GetRoutes(string accessToken)
        {
            var authHeader = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var uri = $"/api/v1/routes.json?page_size={_settings.MaxRoutesCount}";
            var retryCount = 0;

            while (retryCount < 3)
            {
                try
                {
                    var response = await ExecuteGetRequest<RwGpsRoutesListResponse>(uri, authHeader);

                    var routes = response?.Routes;
                    if (routes == null)
                    {
                        return [];
                    }

                    return [.. routes.Select(r => new RwGpsRoute
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Distance = r.Distance,
                        Timestamp = r.Updated_at ?? r.Created_at
                    })];
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

            return [];
        }

        public List<List<double>> SanitizeTrack(List<List<double>> original)
        {
            if (original.Count < 3)
            {
                return original;
            }

            var sanitized = new List<List<double>>();

            if (original[0][0] >= _settings.MinLon && original[0][0] <= _settings.MaxLon &
                original[0][1] >= _settings.MinLat && original[0][1] <= _settings.MaxLat)
            {
                sanitized.Add(original[0]);
            }

            for (var i = 1; i < original.Count - 1; i++)
            {
                var before = original[i - 1];
                var after = original[i + 1];
                var current = original[i];

                var diffXBefore = Math.Abs(current[0] - before[0]);
                var diffYBefore = Math.Abs(current[0] - before[0]);
                var diffXAfter = Math.Abs(current[0] - after[0]);
                var diffYAfter = Math.Abs(current[0] - after[0]);

                var validPoint = diffXBefore < 0.009 &&
                                 diffYBefore < 0.009 &&
                                 diffXAfter < 0.009 &&
                                 diffYAfter < 0.009;

                validPoint = validPoint &&
                                current[0] >= _settings.MinLon && current[0] <= _settings.MaxLon &
                                current[1] >= _settings.MinLat && current[1] <= _settings.MaxLat;


                if (validPoint)
                {
                    sanitized.Add(original[i]);
                }
            }

            if (original[original.Count - 1][0] >= _settings.MinLon && original[original.Count - 1][0] <= _settings.MaxLon &
                original[original.Count - 1][1] >= _settings.MinLat && original[original.Count - 1][1] <= _settings.MaxLat)
            {
                sanitized.Add(original[original.Count - 1]);
            }

            return sanitized;
        }
    }
}
