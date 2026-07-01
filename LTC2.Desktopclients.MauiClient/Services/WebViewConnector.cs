using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LTC2.Desktopclients.MauiClient.Controls;
using LTC2.Desktopclients.MauiClient.Models;

namespace LTC2.Desktopclients.MauiClient.Services
{
    public class WebViewConnector
    {
        public Ltc2WebView WebView { get; set; }

        private string _token;
        private readonly AppSettings _appSettings;

        public string Token => _token;

        public WebViewConnector(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public async Task<string> Login()
        {
            if (_token == null && WebView != null)
            {
                _token = await WebView.GetCookieAsync("token", _appSettings.StartPage);
            }
            return _token;
        }

        public string GetAthleteIdFromToken()
        {
            if (_token == null) return null;
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(_token);
            return jwt.Claims.FirstOrDefault(c => c.Type == "StravaAthleteId")?.Value;
        }

        public async Task<string> GetAthleteIdFromTokenAsync()
        {
            var token = await Login();
            if (token == null) return null;
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.Claims.FirstOrDefault(c => c.Type == "StravaAthleteId")?.Value;
        }

        public void FireOnFileEvent(string fileName)
        {
            try
            {
                var onlyFileName = Path.GetFileName(fileName);
                var jsonMessage = $"{{\"onlyFileName\":\"{onlyFileName}\", \"fileName\":\"{fileName.Replace("\\", "\\\\")}\"}}";
                var script = $"fireOnFileEvent('{jsonMessage}');";
                _ = WebView?.EvaluateJavaScriptAsync(script);
            }
            catch (Exception)
            {
            }
        }
    }
}
