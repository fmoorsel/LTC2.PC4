using Avalonia.Controls;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace LTC2.Desktopclients.AvaloniaClient.Services
{
    public class WebViewConnector
    {
        public NativeWebView WebView { get; set; }

        private string _token;

        public string Token => _token;

        public WebViewConnector()
        {
        }

        public async Task<string> Login()
        {
            if (_token == null && WebView != null)
            {
                var cookieManager = WebView.TryGetCookieManager();

                if (cookieManager != null)
                {
                    var cookies = await cookieManager.GetCookiesAsync();
                    var tokenCookie = cookies.FirstOrDefault(c => c.Name == "token");

                    if (tokenCookie != null)
                    {
                        _token = tokenCookie.Value;
                    }
                }
            }

            return _token;
        }

        public string GetAthleteIdFromToken()
        {
            if (_token == null) return null;

            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(_token);

            return jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == "StravaAthleteId")?.Value;
        }

        public async Task<string> GetAthleteIdFromTokenAsync()
        {
            var token = await Login();

            if (token == null) return null;

            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(token);

            return jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == "StravaAthleteId")?.Value;
        }

        public void FireOnFileEvent(string fileName)
        {
            try
            {
                var onlyFileName = System.IO.Path.GetFileName(fileName);
                var jsonMessage = $"{{\"onlyFileName\":\"{onlyFileName}\", \"fileName\":\"{fileName.Replace("\\", "\\\\")}\"}}";
                var script = $"fireOnFileEvent('{jsonMessage}');";

                WebView?.InvokeScript(script);
            }
            catch (Exception)
            {
            }
        }
    }
}
