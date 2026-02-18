using LTC2.Shared.Models.Settings;
using LTC2.Shared.RideWithGpsConnector.Interfaces;
using LTC2.Webapps.MainApp.Models;
using LTC2.Webapps.MainApp.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LTC2.Webapps.MainApp.Controllers
{
    [Authorize]
    [AllowAnonymous]
    public class HomeRideWithGpsController : Controller
    {
        private readonly RideWithGpsHttpProxySettings _rideWithGpsHttpProxySettings;
        private readonly IRideWithGpsConnector _rideWithGpsConnector;
        private readonly TokenUtils _tokenUtils;
        private readonly AppSettings _appSettings;
        private readonly ILogger<HomeRideWithGpsController> _logger;

        private readonly string _stateCookieName = "rwgps_state";
        private readonly string _appEntrypoint = "../app/index.html";

        public HomeRideWithGpsController(
            RideWithGpsHttpProxySettings rideWithGpsHttpProxySettings,
            IRideWithGpsConnector rideWithGpsConnector,
            TokenUtils tokenUtils,
            AppSettings appSettings,
            ILogger<HomeRideWithGpsController> logger)
        {
            _rideWithGpsHttpProxySettings = rideWithGpsHttpProxySettings;
            _rideWithGpsConnector = rideWithGpsConnector;
            _tokenUtils = tokenUtils;
            _appSettings = appSettings;
            _logger = logger;
        }

        public IActionResult Index(string language, bool multi)
        {
            var tokenCookie = HttpContext.Request.Cookies[TokenUtils.TokenName];

            if (!string.IsNullOrEmpty(tokenCookie))
            {
                var validUntil = _tokenUtils.TokenIsValidUntil(tokenCookie);

                if (validUntil >= DateTime.UtcNow.AddHours(1))
                {
                    return Redirect(_appEntrypoint + $"?t={DateTime.UtcNow.Ticks}");
                }
            }

            var state = Guid.NewGuid().ToString();

            HttpContext.Response.Cookies.Append(_stateCookieName, state);

            ViewBag.RwGpsClientId = _rideWithGpsHttpProxySettings.ClientId;
            ViewBag.State = state;
            ViewBag.AppEntryPoint = _appEntrypoint + $"?t={DateTime.UtcNow.Ticks}";

            return View();
        }

        public async Task<IActionResult> CallbackConnect(string code, string state, string language = "nl")
        {
            _logger.LogInformation("Ride With GPS OAuth callback received");

            var isStateOk = CheckState(state);

            var expiredCookieOptions = new CookieOptions()
            {
                Expires = DateTime.Now.AddDays(-1)
            };

            HttpContext.Response.Cookies.Append(_stateCookieName, "", expiredCookieOptions);

            if (!isStateOk)
            {
                _logger.LogWarning("Ride With GPS OAuth state validation failed");

                return RedirectToAction("Index");
            }

            if (string.IsNullOrEmpty(code))
            {
                _logger.LogError("Ride With GPS OAuth callback received without code");

                return Unauthorized();
            }

            try
            {
                var session = await _rideWithGpsConnector.GetSession(code);

                if (session == null || session.AthleteId == -1)
                {
                    _logger.LogError("Error getting session from Ride With GPS");

                    return Unauthorized();
                }

                var token = _tokenUtils.GenerateToken(session.AthleteId.ToString(), string.Empty, session.Athlete.Name);

                var cookieOptions1 = new CookieOptions()
                {
                    IsEssential = true,
                    SameSite = SameSiteMode.Strict,
                    Secure = _appSettings.UseRedirectDuringLogin
                };

                HttpContext.Response.Cookies.Append(TokenUtils.TokenName, token, cookieOptions1);

                if (!_appSettings.UseRedirectDuringLogin)
                {
                    var cookieOptions2 = new CookieOptions()
                    {
                        IsEssential = true,
                        SameSite = SameSiteMode.None,
                        Secure = true
                    };

                    HttpContext.Response.Cookies.Append(TokenUtils.TokenName, token, cookieOptions2);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception while getting session from Ride With GPS");

                return Unauthorized();
            }

            if (_appSettings.UseRedirectDuringLogin)
            {
                return Redirect(_appEntrypoint + $"?t={DateTime.UtcNow.Ticks}");
            }
            else
            {
                ViewBag.Entrypoint = _appEntrypoint + $"?t={DateTime.UtcNow.Ticks}";

                return View("CompleteLogin");
            }
        }

        private bool CheckState(string state)
        {
            var result = false;

            var stateCookie = HttpContext.Request.Cookies[_stateCookieName];

            if (!string.IsNullOrEmpty(stateCookie) && !string.IsNullOrEmpty(state))
            {
                result = stateCookie == state;
            }

            return result;
        }
    }
}
