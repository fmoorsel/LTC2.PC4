using LTC2.Shared.BaseMessages.Interfaces;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.StravaConnector.Interfaces;
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
    public class HomeController : Controller
    {
        private readonly StravaHttpProxySettings _stravaHttpProxySettings;
        private readonly TokenUtils _tokenUtils;
        private readonly IStravaConnector _stravaConnector;
        private readonly ProfileManager _profileManager;
        private readonly IBaseTranslationService _baseTranslationService;
        private readonly ILogger<HomeController> _logger;

        private readonly string _stateCookieName = "state";
        private readonly string _languageCookieName = "language";
        private readonly string _appEntrypoint = "../app/index.html";

        private readonly AppSettings _appSettings;

        public static string MULTI_COOKIE_NAME = "multi";
        public static string MULTI_COOKIE_VALUE = "multi";

        public HomeController(
            StravaHttpProxySettings stravaHttpProxySettings,
            ProfileManager profileManager,
            TokenUtils tokenUtils,
            IBaseTranslationService baseTranslationService,
            IStravaConnector stravaConnector,
            ILogger<HomeController> logger,
            AppSettings appSettings)
        {
            _stravaHttpProxySettings = stravaHttpProxySettings;
            _tokenUtils = tokenUtils;
            _stravaConnector = stravaConnector;
            _profileManager = profileManager;
            _baseTranslationService = baseTranslationService;
            _appSettings = appSettings;
            _logger = logger;
        }

        public IActionResult Index(bool forceLogout, string profile, string language, bool multi, string source)
        {
            if (source == "ridewithgps")
            {
                return RedirectToAction("Index", "HomeRideWithGps", new { forceLogout, language, multi, profile, source });
            }

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
                var tokenCookie = HttpContext.Request.Cookies["token"];

                if (!string.IsNullOrEmpty(tokenCookie))
                {
                    var validUntil = _tokenUtils.TokenIsValidUntil(tokenCookie);

                    if (validUntil >= DateTime.UtcNow.AddHours(1))
                    {
                        return Redirect(_appEntrypoint + $"?strava={DateTime.UtcNow.Ticks}strava");
                    }
                }
            }

            ViewBag.TestProfile = testProfile;
            ViewBag.ApprovalPrompt = approvalPrompt;
            ViewBag.StravaAppId = _stravaHttpProxySettings.ClientId;
            ViewBag.State = testProfile ? $"{state},true" : state;
            ViewBag.Language = language;

            HttpContext.Response.Cookies.Append(_stateCookieName, state);
            HttpContext.Response.Cookies.Append(_languageCookieName, language ?? _baseTranslationService.CurrentLanguage);
            HttpContext.Response.Cookies.Append(MULTI_COOKIE_NAME, multi ? MULTI_COOKIE_VALUE : string.Empty);

            ViewBag.AppEntryPoint = _appEntrypoint + $"?strava={DateTime.UtcNow.Ticks}";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public async Task<IActionResult> CallBackConnect(string code, string state, string language = "nl", string scope = "")
        {
            var testProfile = false;

            ViewBag.Code = code;
            ViewBag.ScopeOk = CheckScope(scope);

            if (state.EndsWith(",true"))
            {
                state = state.Split(',')[0];
                testProfile = true;
            }

            var isStateOk = CheckState(state);

            var cookieOptions = new CookieOptions()
            {
                Expires = DateTime.Now.AddDays(-1)
            };

            HttpContext.Response.Cookies.Append(_stateCookieName, "", cookieOptions);

            if (isStateOk)
            {
                return await Login(code, testProfile);
            }
            else
            {
                return RedirectToAction("Home");
            }
        }

        private bool CheckScope(string scope)
        {
            var result = true;

            if (!string.IsNullOrEmpty(scope))
            {
                if (scope.IndexOf("activity:read_all") < 0)
                {
                    result = false;
                }

                if (scope.IndexOf("profile:read_all") < 0)
                {
                    result = false;
                }
            }

            return result;
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

        private async Task<IActionResult> Login(string code, bool testProfile)
        {
            try
            {
                _logger.LogInformation("Getting session from Strava");

                if (code == null)
                {
                    _logger.LogError("Error getting session from Strava: no code!");

                    return Unauthorized();
                }

                var session = await _stravaConnector.GetSession(code);

                if (session == null || session.AthleteId == -1)
                {
                    _logger.LogError("Error getting session from Strava");

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
                _logger.LogError(e, "Exception while getting session from Strava");

                return Unauthorized();
            }

            if (_appSettings.UseRedirectDuringLogin)
            {
                return Redirect(_appEntrypoint + $"?strava={DateTime.UtcNow.Ticks}");
            }
            else
            {
                ViewBag.Entrypoint = _appEntrypoint + $"?strava={DateTime.UtcNow.Ticks}";

                return View("CompleteLogin");
            }
        }

    }
}
