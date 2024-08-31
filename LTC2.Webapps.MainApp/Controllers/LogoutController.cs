using LTC2.Webapps.MainApp.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LTC2.Webapps.MainApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogoutController : ControllerBase
    {
        private readonly TokenUtils _tokenUtils;

        public LogoutController(TokenUtils tokenUtils)
        {
            _tokenUtils = tokenUtils;
        }

        [HttpGet]
        [Route("logout")]
        public IActionResult Logout()
        {
            var tokenCookie = HttpContext.Request.Cookies[TokenUtils.TokenName];

            if (tokenCookie != null)
            {
                var cookieOptions = new CookieOptions()
                {
                    IsEssential = true,
                    SameSite = SameSiteMode.Strict,
                    Secure = true,
                    Expires = DateTime.Now.AddDays(-1)
                };

                HttpContext.Response.Cookies.Append(TokenUtils.TokenName, "", cookieOptions);
            };


            return Ok();
        }
    }
}
