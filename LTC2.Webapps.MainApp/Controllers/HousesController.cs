using LTC2.Shared.ActivityFormats.Gpx.Utils;
using LTC2.Shared.Http.Interfaces;
using LTC2.Shared.Repositories.Interfaces;
using LTC2.Webapps.MainApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LTC2.Webapps.MainApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HousesController : ControllerBase
    {
        private readonly FundaClient _fundaClient;
        private readonly IPdokLocatieserverProxy _pdokProxy;
        private readonly IMapRepository _mapRepository;
        private readonly AppSettings _appSettings;
        private readonly ILogger<HousesController> _logger;

        // WKT POINT(lon lat) from PDOK centroide_ll
        private static readonly Regex PointWktRegex = new(@"POINT\(([0-9.]+)\s+([0-9.]+)\)", RegexOptions.Compiled);

        // Factory — same SRID as track geometries (WGS84 / 4326)
        private static readonly GeometryFactory GeoFactory =
            LTC2.Shared.Utils.Utils.GeometryProducer.Instance.GetFactory();

        // 50 m in degrees (111 320 m/° at equator — close enough for NL).
        // Euclidean NTS distance on WGS84 introduces ≤38 % error on E–W bearing,
        // giving an effective envelope of 35–70 m. Acceptable for route proximity.
        private const double MaxDistanceMeters  = 50.0;
        private const double MetersPerDegree    = 111_320.0;
        private const double MaxDistanceDegrees = MaxDistanceMeters / MetersPerDegree;


        private bool _isMapRepositoryOpen = false;
        private readonly object _mapRepositoryLock = new object();

        public HousesController(
            FundaClient fundaClient,
            IPdokLocatieserverProxy pdokProxy,
            IMapRepository mapRepository,
            AppSettings appSettings,
            ILogger<HousesController> logger)
        {
            _fundaClient = fundaClient;
            _pdokProxy   = pdokProxy;
            _mapRepository = mapRepository;
            _appSettings = appSettings;
            _logger      = logger;
        }

        // ── Endpoint 1: JSON list ──────────────────────────────────────────────
        // POST /api/houses
        // Returns List<HouseListing> filtered to houses within 50 m of the route.

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetHousesAlongRoute(IFormFile file, CancellationToken ct)
        {
            var (tracks, listings, error) = await FetchListingsAsync(file, ct);
            if (error != null) return error;

            var filtered = listings
                .Where(h => IsWithinRoute(h, tracks))
                .ToList();

            return Ok(filtered);
        }

        // ── Endpoint 2: GeoJSON ────────────────────────────────────────────────
        // POST /api/houses/geojson?doFilter=true|false
        // Returns a GeoJSON FeatureCollection with:
        //   • The GPX track(s) as LineString features
        //   • Each house as a Polygon circle (radius = 50 m) with house properties
        // When doFilter=true only houses within 50 m of the track are included.

        [HttpPost]
        [Authorize]
        [Route("geojson")]
        public async Task<IActionResult> GetHousesAsGeoJson(
            IFormFile file, [FromQuery] bool doFilter, CancellationToken ct)
        {
            var (tracks, listings, error) = await FetchListingsAsync(file, ct);
            if (error != null) return error;

            var features = new List<object>();

            // Track features
            foreach (var track in tracks)
                features.Add(BuildTrackFeature(track));

            // House features (circles)
            foreach (var house in listings)
            {
                bool withinRoute = IsWithinRoute(house, tracks);
                if (doFilter && !withinRoute) continue;
                features.Add(BuildHouseFeature(house, withinRoute));
            }

            var featureCollection = new { type = "FeatureCollection", features };

            return new JsonResult(featureCollection) { ContentType = "application/geo+json" };
        }

        // ── Shared pipeline ────────────────────────────────────────────────────

        private async Task<(List<LineString> Tracks, List<HouseListing> Listings, IActionResult Error)>
            FetchListingsAsync(IFormFile file, CancellationToken ct)
        {
            if (file == null || file.Length == 0)
                return (null, null, BadRequest("A GPX file is required."));

            string gpxFile;
            try
            {
                gpxFile = SaveGpxFile(file);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error saving GPX file");
                return (null, null, StatusCode(500, "Failed to process GPX file."));
            }

            List<LineString> tracks;
            List<string> areaIds;
            try
            {
                (tracks, areaIds) = ExtractGpxData(gpxFile);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error analysing GPX file");
                return (null, null, StatusCode(500, "Failed to analyse GPX file."));
            }

            if (areaIds.Count == 0)
                return (tracks, new List<HouseListing>(), null);

            // Funda: batches of 3–5 areas with 2–4 s delay between calls
            var allListings = new List<FundaListing>();
            var batches = CreateBatches(areaIds);

            for (int i = 0; i < batches.Count; i++)
            {
                if (ct.IsCancellationRequested) break;

                var location = string.Join(",", batches[i]);
                _logger.LogInformation("Funda batch {Index}/{Total}: {Location}", i + 1, batches.Count, location);

                try
                {
                    var results = await _fundaClient.SearchListingsAsync(
                        location, offeringType: "buy", ct: ct);
                    allListings.AddRange(results);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Funda batch {Index} failed, skipping", i + 1);
                }

                if (i < batches.Count - 1)
                    await Task.Delay(Random.Shared.Next(2000, 4001), ct);
            }

            // Deduplicate by GlobalId
            var seen   = new HashSet<long?>();
            var unique = new List<FundaListing>();
            foreach (var l in allListings)
                if (seen.Add(l.GlobalId))
                    unique.Add(l);

            // PDOK geocode each listing
            var listings = new List<HouseListing>(unique.Count);
            foreach (var l in unique)
            {
                if (ct.IsCancellationRequested) break;
                var dto = MapToDto(l);
                await EnrichWithPdokCoordinatesAsync(dto, l, ct);
                listings.Add(dto);
            }

            return (tracks, listings, null);
        }

        // ── GeoJSON builders ───────────────────────────────────────────────────

        // Track coordinate convention in this project: Coordinate(lat, lon) → X=lat, Y=lon.
        // GeoJSON requires [longitude, latitude], so every coordinate is output as [Y, X].

        private static object BuildTrackFeature(LineString track)
        {
            var coords = track.Coordinates
                .Select(c => new[] { c.Y, c.X })   // [lon, lat]
                .ToArray();

            return new
            {
                type     = "Feature",
                geometry = new { type = "LineString", coordinates = coords },
                properties = new { featureType = "track" }
            };
        }

        private static object BuildHouseFeature(HouseListing house, bool withinRoute)
        {
            double[][] ring = null;
            if (house.Latitude.HasValue && house.Longitude.HasValue)
                ring = BuildRectangleRing(house.Latitude.Value, house.Longitude.Value, MaxDistanceMeters);

            return new
            {
                type     = "Feature",
                geometry = ring != null
                    ? (object)new { type = "Polygon", coordinates = new[] { ring } }
                    : null,
                properties = new
                {
                    featureType    = "house",
                    address        = house.Address,
                    postcode       = house.Postcode,
                    city           = house.City,
                    price          = house.Price,
                    priceFormatted = house.PriceFormatted,
                    url            = house.Url,
                    bedrooms       = house.Bedrooms,
                    livingArea     = house.LivingArea,
                    energyLabel    = house.EnergyLabel,
                    status         = house.Status,
                    withinRoute,
                }
            };
        }

        /// <summary>
        /// Returns a closed GeoJSON polygon ring for a rectangle centred on (lat, lon)
        /// with each side exactly <paramref name="halfSideMeters"/> metres from the centre.
        /// Longitude offset is scaled by cos(lat) so all four sides are equal on the ground.
        /// Coordinates are in GeoJSON order [longitude, latitude].
        /// </summary>
        private static double[][] BuildRectangleRing(double lat, double lon, double halfSideMeters)
        {
            double dLat = halfSideMeters / MetersPerDegree;
            double dLon = halfSideMeters / (MetersPerDegree * Math.Cos(lat * Math.PI / 180.0));

            double north = lat + dLat;
            double south = lat - dLat;
            double east  = lon + dLon;
            double west  = lon - dLon;

            // GeoJSON rings are closed: first == last coordinate
            return new[]
            {
                new[] { west,  north },   // NW
                new[] { east,  north },   // NE
                new[] { east,  south },   // SE
                new[] { west,  south },   // SW
                new[] { west,  north },   // close
            };
        }

        // ── GPX parsing ────────────────────────────────────────────────────────

        private (List<LineString> Tracks, List<string> AreaIds) ExtractGpxData(string gpxFile)
        {
            EnsureMapRepository();

            var tracks  = GpxCoordinateUtils.CreateLinestringForGpxTrack(gpxFile);
            var areaIds = new HashSet<string>();

            foreach (var track in tracks)
            {
                var coordinates = track.Coordinates
                    .Select(c => new List<double> { c.Y, c.X })   // [lon, lat] for CheckTrack
                    .ToList();

                var places = _mapRepository.CheckTrack(coordinates);
                foreach (var place in places)
                    areaIds.Add(place.Name);
            }

            return (tracks, areaIds.ToList());
        }

        // ── Proximity filter ───────────────────────────────────────────────────

        private static bool IsWithinRoute(HouseListing house, List<LineString> tracks)
        {
            if (!house.Latitude.HasValue || !house.Longitude.HasValue)
                return false;

            var housePoint = GeoFactory.CreatePoint(
                new Coordinate(house.Latitude.Value, house.Longitude.Value));

            return tracks.Any(track => track.Distance(housePoint) <= MaxDistanceDegrees);
        }

        // ── PDOK geocoding ─────────────────────────────────────────────────────

        private async Task EnrichWithPdokCoordinatesAsync(
            HouseListing dto, FundaListing source, CancellationToken ct)
        {
            var address  = source.Title?.Trim();
            var postcode = source.Postcode?.Trim();
            var city     = source.City?.Trim();

            if (string.IsNullOrWhiteSpace(address) &&
                string.IsNullOrWhiteSpace(postcode) &&
                string.IsNullOrWhiteSpace(city))
                return;

            var query = string.Join(" ",
                new[] { address, postcode, city }.Where(s => !string.IsNullOrWhiteSpace(s)));

            try
            {
                var result = await _pdokProxy.FreeAsync(query, rows: 1, filterQuery: "type:adres");
                var doc    = result?.Response?.Docs?.FirstOrDefault();
                if (doc?.CentroidLL == null) return;

                // WKT: POINT(lon lat) — group 1 = longitude, group 2 = latitude
                var match = PointWktRegex.Match(doc.CentroidLL);
                if (!match.Success) return;

                dto.Longitude = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                dto.Latitude  = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "PDOK geocoding failed for '{Query}'", query);
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static List<List<string>> CreateBatches(List<string> ids)
        {
            var batches = new List<List<string>>();
            int index   = 0;
            while (index < ids.Count)
            {
                int size = Random.Shared.Next(3, 6);
                batches.Add(ids.Skip(index).Take(size).ToList());
                index += size;
            }
            return batches;
        }

        private string SaveGpxFile(IFormFile file)
        {
            if (!Directory.Exists(_appSettings.TempRoutesFolder))
                Directory.CreateDirectory(_appSettings.TempRoutesFolder);

            var path = Path.Combine(_appSettings.TempRoutesFolder, $"{Guid.NewGuid()}.gpx");
            using var fs = System.IO.File.Create(path);
            file.CopyTo(fs);
            fs.Flush();
            return path;
        }

        private void EnsureMapRepository()
        {
            lock (_mapRepositoryLock)
            {
                if (!_isMapRepositoryOpen)
                {
                    _mapRepository.Open();
                    _isMapRepositoryOpen = true;
                }
            }
        }

        private static HouseListing MapToDto(FundaListing l) => new HouseListing
        {
            GlobalId         = l.GlobalId,
            TinyId           = l.TinyId,
            Url              = l.Url,
            Address          = l.Address,
            City             = l.City,
            Postcode         = l.Postcode,
            Neighbourhood    = l.Neighbourhood,
            Municipality     = l.Municipality,
            Price            = l.Price,
            PriceFormatted   = l.PriceFormatted,
            Status           = l.Status,
            OfferingType     = l.OfferingType,
            Bedrooms         = l.Bedrooms,
            Rooms            = l.Rooms,
            LivingArea       = l.LivingArea,
            PlotArea         = l.PlotArea,
            EnergyLabel      = l.EnergyLabel,
            ObjectType       = l.ObjectType,
            HouseType        = l.HouseType,
            ConstructionYear = l.ConstructionYear,
            Latitude         = l.Latitude,
            Longitude        = l.Longitude,
            PublicationDate  = l.PublicationDate,
            PhotoUrls        = l.PhotoUrls,
            HasGarden        = l.HasGarden,
            HasBalcony       = l.HasBalcony,
            HasRoofTerrace   = l.HasRoofTerrace,
            HasSolarPanels   = l.HasSolarPanels,
            HasHeatPump      = l.HasHeatPump,
            IsMonument       = l.IsMonument,
            IsAuction        = l.IsAuction,
        };
    }
}
