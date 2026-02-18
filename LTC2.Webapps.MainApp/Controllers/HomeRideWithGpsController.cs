using LTC2.Shared.BaseMessages.Interfaces;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.RideWithGpsConnector.Interfaces;
using LTC2.Webapps.MainApp.Models;
using LTC2.Webapps.MainApp.Services;
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
        private readonly ProfileManager _profileManager;
        private readonly IBaseTranslationService _baseTranslationService;
        private readonly ILogger<HomeRideWithGpsController> _logger;

        private readonly string _stateCookieName = "rwgps_state";
        private readonly string _languageCookieName = "language";
        private readonly string _appEntrypoint = "../app/index.html";

        public HomeRideWithGpsController(
            RideWithGpsHttpProxySettings rideWithGpsHttpProxySettings,
            IRideWithGpsConnector rideWithGpsConnector,
            TokenUtils tokenUtils,
            AppSettings appSettings,
            ProfileManager profileManager,
            IBaseTranslationService baseTranslationService,
            ILogger<HomeRideWithGpsController> logger)
        {
            _rideWithGpsHttpProxySettings = rideWithGpsHttpProxySettings;
            _rideWithGpsConnector = rideWithGpsConnector;
            _tokenUtils = tokenUtils;
            _appSettings = appSettings;
            _profileManager = profileManager;
            _baseTranslationService = baseTranslationService;
            _logger = logger;
        }

        public IActionResult Index(bool forceLogout, string language, bool multi, string profile)
        {
            var state = Guid.NewGuid().ToString();
            var testProfile = false;
            var approvalPrompt = "auto";

            if (forceLogout)
            {
                var cookieOptions = new CookieOptions()
                {
                    Expires = DateTime.Now.AddDays(-1)
                };

                HttpContext.Response.Cookies.Append(TokenUtils.TokenName, "", cookieOptions);
            }
            else if (profile != null)
            {
                var success = _profileManager.ActivateProfile(profile, true);

                if (!success)
                {
                    return RedirectToAction("Error");
                }

                testProfile = true;
                approvalPrompt = "forced";
            }
            else
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
            }

            HttpContext.Response.Cookies.Append(_stateCookieName, state);
            HttpContext.Response.Cookies.Append(_languageCookieName, language ?? _baseTranslationService.CurrentLanguage);
            HttpContext.Response.Cookies.Append(HomeController.MULTI_COOKIE_NAME, multi ? HomeController.MULTI_COOKIE_VALUE : string.Empty);

            ViewBag.RwGpsClientId = _rideWithGpsHttpProxySettings.ClientId;
            ViewBag.State = testProfile ? $"{state},true" : state;
            ViewBag.ApprovalPrompt = approvalPrompt;
            ViewBag.Language = language;
            ViewBag.AppEntryPoint = _appEntrypoint + $"?t={DateTime.UtcNow.Ticks}";

            return View();
        }

        public async Task<IActionResult> CallbackConnect(string code, string state, string language = "nl")
        {
            _logger.LogInformation("Ride With GPS OAuth callback received");

            var testProfile = false;

            if (state != null && state.EndsWith(",true"))
            {
                state = state.Split(',')[0];
                testProfile = true;
            }

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

            return await Login(code, testProfile);
        }

        private async Task<IActionResult> Login(string code, bool testProfile)
        {
            try
            {
                _logger.LogInformation("Getting session from Ride With GPS");

                if (string.IsNullOrEmpty(code))
                {
                    _logger.LogError("Error getting session from Ride With GPS: no code!");

                    return Unauthorized();
                }

                var redirectUri = Url.Action("CallbackConnect", "HomeRideWithGps", null, Request.Scheme);
                var session = await _rideWithGpsConnector.GetSession(code, redirectUri);

                if (session == null || session.AthleteId == -1)
                {
                    _logger.LogError("Error getting session from Ride With GPS");

                    return Unauthorized();
                }

                ViewBag.Name = session.Athlete.Name;
                ViewBag.AthleteId = session.AthleteId;

                if (!testProfile)
                {
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
                else
                {
                    return View("Login");
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
