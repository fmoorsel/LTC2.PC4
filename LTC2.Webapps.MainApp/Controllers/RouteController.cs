using LTC2.Shared.ActivityFormats.Gpx.Utils;
using LTC2.Shared.Common.Interfaces;
using LTC2.Shared.Models.Domain;
using LTC2.Shared.Models.Requests;
using LTC2.Shared.Models.Responses;
using LTC2.Shared.Repositories.Interfaces;
using LTC2.Shared.StravaConnector.Exceptions;
using LTC2.Shared.StravaConnector.Interfaces;
using LTC2.Webapps.MainApp.Models;
using LTC2.Webapps.MainApp.Models.Requests;
using LTC2.Webapps.MainApp.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LTC2.Webapps.MainApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RouteController : ControllerBase
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger<RouteController> _logger;
        private readonly IMapRepository _mapRepository;
        private readonly TokenUtils _tokenUtils;
        private readonly IStravaConnector _stravaConnector;
        private readonly IConnectorFactory _connectorFactory;

        private bool _isMapRepositoryOpen = false;
        private object _mapRepositoryLock = new object();

        public RouteController(
            TokenUtils tokenUtils,
            ILogger<RouteController> logger,
            IMapRepository mapRepository,
            IStravaConnector stravaConnector,
            IStravaHttpProxy stravaHttpProxy,
            IConnectorFactory connectorFactory,
            AppSettings appSettings)
        {
            _appSettings = appSettings;
            _mapRepository = mapRepository;
            _tokenUtils = tokenUtils;
            _stravaConnector = stravaConnector;
            _connectorFactory = connectorFactory;
            _logger = logger;
        }

        [HttpPost]
        [Authorize]
        [Route("checkgpx")]
        public IActionResult CheckGpx(IFormFile file)
        {
            var fileName = ReadGpxFile(file);

            var response = CheckGpxFile(fileName);

            return Ok(response);
        }

        [HttpGet]
        [Authorize]
        [Route("checkgpxfrompath")]
        public IActionResult CheckGpxFromPath([FromQuery] string file)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var base64EncodedBytes = Convert.FromBase64String(file);
                var decodedString = Encoding.UTF8.GetString(base64EncodedBytes);

                var fileName = ReadGpxFile(decodedString);

                var response = CheckGpxFile(fileName);

                return Ok(response);
            }
            ;

            return Unauthorized();
        }

        [HttpGet]
        [Authorize]
        [Route("checkstravaroute")]
        public async Task<IActionResult> CheckStravaRoute([FromQuery] CheckStravaRouteRequest request)
        {
            return await CheckSourceRouteInternal(request.RouteId, ConnectorSource.Strava);
        }

        [HttpGet]
        [Authorize]
        [Route("checkrwgpsroute")]
        public async Task<IActionResult> CheckRwGpsRoute([FromQuery] CheckStravaRouteRequest request)
        {
            return await CheckSourceRouteInternal(request.RouteId, ConnectorSource.RideWithGps);
        }

        [HttpGet]
        [Authorize]
        [Route("checksourceroute")]
        public async Task<IActionResult> CheckSourceRoute([FromQuery] CheckStravaRouteRequest request, [FromQuery] string source)
        {
            var connectorSource = source == "ridewithgps" ? ConnectorSource.RideWithGps : ConnectorSource.Strava;

            return await CheckSourceRouteInternal(request.RouteId, connectorSource);
        }

        private async Task<IActionResult> CheckSourceRouteInternal(string routeId, ConnectorSource connectorSource)
        {
            var authHeader = _tokenUtils.GetAuthenticationHeader(HttpContext.Request);
            var token = authHeader?.Parameter;

            if (token != null)
            {
                if (_tokenUtils.ValidateToken(token))
                {
                    var athleteId = _tokenUtils.GetProfileFormToken(token).AthleteId;

                    var gpxRequest = new GetRouteDetailsAsGpxRequest();
                    gpxRequest.AthleteId = Convert.ToInt64(athleteId);
                    gpxRequest.RouteId = Convert.ToInt64(routeId);

                    var connector = _connectorFactory.Create(connectorSource);

                    try
                    {
                        var gpx = await connector.GetRouteDetailsAsGpx(gpxRequest);

                        if (gpx.LimitsExceeded)
                        {
                            var response = new Routes();
                            response.LimitInfo = new LimitInfo();

                            response.LimitInfo.LimitsExceeded = true;

                            if (gpx.HasLimits)
                            {
                                response.LimitInfo.QuarterRateLimit = gpx.QuarterRateLimit;
                                response.LimitInfo.QuarterRateUsage = gpx.QuarterRateUsage;
                                response.LimitInfo.DayRateLimit = gpx.DayRateLimit;
                                response.LimitInfo.DayRateUsage = gpx.DayRateUsage;
                            }

                            return Ok(response);
                        }

                        if (gpx != null && gpx.Gpx != null)
                        {
                            var gpxFile = WriteGpxFile(gpx.Gpx);
                            var response = CheckGpxFile(gpxFile);

                            response.IsStravaRoute = true;
                            response.StravaRouteId = routeId;

                            return Ok(response);
                        }
                    }
                    catch (StravaTooManyDailyRequestsException ex)
                    {
                        var response = new Routes();
                        response.LimitInfo = new LimitInfo();

                        response.LimitInfo.LimitsExceeded = true;

                        if (ex.Limits.HasLimits)
                        {
                            response.LimitInfo.QuarterRateLimit = ex.Limits.QuarterRateLimit;
                            response.LimitInfo.QuarterRateUsage = ex.Limits.QuarterRateUsage;
                            response.LimitInfo.DayRateLimit = ex.Limits.DayRateLimit;
                            response.LimitInfo.DayRateUsage = ex.Limits.DayRateUsage;
                        }

                        return Ok(response);
                    }

                    return NotFound();
                }
            }

            return Unauthorized();
        }

        [HttpGet]
        [Authorize]
        [Route("centerpoint")]
        public IActionResult GetCenterPointForName([FromQuery] string name)
        {
            EnsureMapRepository();

            var result = _mapRepository.GetCenterPointForName(name);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpGet]
        [Authorize]
        [Route("list")]
        public async Task<IActionResult> GetRoutes([FromQuery] string source)
        {
            var authHeader = _tokenUtils.GetAuthenticationHeader(HttpContext.Request);
            var token = authHeader?.Parameter;

            if (token != null)
            {
                if (_tokenUtils.ValidateToken(token))
                {
                    var athleteId = _tokenUtils.GetProfileFormToken(token).AthleteId;

                    var request = new GetRoutesRequest();
                    request.AthleteId = Convert.ToInt64(athleteId);

                    var connectorSource = source == "ridewithgps" ? ConnectorSource.RideWithGps : ConnectorSource.Strava;
                    var connector = _connectorFactory.Create(connectorSource);
                    var routes = await connector.GetRoutes(request);

                    return Ok(routes);
                }
            }

            return Unauthorized();
        }

        private string WriteGpxFile(string gpx)
        {
            EnsureTempFolderExists();


            try
            {
                var uniqueId = Guid.NewGuid().ToString();
                var gpxName = Path.Combine(_appSettings.TempRoutesFolder, $"{uniqueId}.gpx");

                System.IO.File.WriteAllText(gpxName, gpx);

                return gpxName;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error writing gpx file");

                throw;
            }
        }

        private string ReadGpxFile(string file)
        {
            EnsureTempFolderExists();

            CleanOldGpxFiles();

            try
            {
                var uniqueId = Guid.NewGuid().ToString();
                var gpxName = Path.Combine(_appSettings.TempRoutesFolder, $"{uniqueId}.gpx");

                System.IO.File.Copy(file, gpxName);

                return gpxName;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error reading gpx file");

                throw;
            }

        }

        private string ReadGpxFile(IFormFile file)
        {
            EnsureTempFolderExists();

            CleanOldGpxFiles();

            try
            {
                var uniqueId = Guid.NewGuid().ToString();
                var gpxName = Path.Combine(_appSettings.TempRoutesFolder, $"{uniqueId}.gpx");

                using (FileStream fs = System.IO.File.Create(gpxName))
                {
                    file.CopyTo(fs);

                    fs.Flush();
                }

                return gpxName;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error reading gpx file");

                throw;
            }
        }

        private Routes CheckGpxFile(string file)
        {
            try
            {
                EnsureMapRepository();

                var tracks = GpxCoordinateUtils.CreateLinestringForGpxTrack(file);
                var response = new Routes();

                foreach (var track in tracks)
                {
                    // check each track and consolidate results
                    var coordinates = new List<List<double>>();

                    foreach (var trackCoordinate in track.Coordinates)
                    {
                        var coordinate = new List<double>
                        {
                            trackCoordinate.Y,
                            trackCoordinate.X
                        };

                        coordinates.Add(coordinate);
                    }

                    var places = _mapRepository.CheckTrack(coordinates);

                    var route = new Route
                    {
                        Coordinates = coordinates,
                        Places = places.Select(p => p.Id).ToList()
                    };

                    response.RouteCollection.Add(route);
                }

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error checking gpx file");

                throw;
            }
        }

        private void EnsureTempFolderExists()
        {
            try
            {
                if (!Directory.Exists(_appSettings.TempRoutesFolder))
                {
                    Directory.CreateDirectory(_appSettings.TempRoutesFolder);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error ensuring temp folder");

                throw;
            }
        }

        private void CleanOldGpxFiles()
        {
            try
            {
                var files = Directory.GetFiles(_appSettings.TempRoutesFolder, "*.gpx");

                foreach (var file in files)
                {
                    var expired = System.IO.File.GetCreationTime(file).ToUniversalTime().AddHours(6);
                    var now = DateTime.UtcNow;

                    if (now > expired)
                    {
                        System.IO.File.Delete(file);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error cleaning old gpx files");

                throw;
            }
        }

        private void EnsureMapRepository()
        {
            lock (_mapRepositoryLock)
            {
                if (!_isMapRepositoryOpen)
                {
                    _mapRepository.Open();

                    _isMapRepositoryOpen = true;

                    _logger.LogInformation("Map repository opened");
                }
            }
        }
    }
}
