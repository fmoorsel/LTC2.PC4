using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls;

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
                var result = await WebView.InvokeScript("getToken();");

                if (result != null && result.Length > 2 && result.StartsWith('"') && result.EndsWith('"'))
                {
                    var unescaped = Regex.Unescape(result);
                    _token = unescaped.Substring(1, unescaped.Length - 2);
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

        public void DeleteCookies(string domain)
        {
            WebView?.InvokeScript(
                "document.cookie.split(';').forEach(c => { document.cookie = c.replace(/^ +/, '').replace(/=.*/, '=;expires=' + new Date().toUTCString() + ';path=/'); });");
        }

        public void DeleteStravaCookies()
        {
            DeleteCookies("strava.com");
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
