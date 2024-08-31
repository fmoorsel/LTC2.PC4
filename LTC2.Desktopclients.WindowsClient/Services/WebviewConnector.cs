using Microsoft.Web.WebView2.WinForms;
using System.Text.RegularExpressions;

namespace LTC2.Desktopclients.WindowsClient.Services
{
    public class WebviewConnector
    {
        public WebView2 WebView { get; set; }

        private string _token;

        public WebviewConnector()
        {
        }

        public async Task<string> Login()
        {
            if (_token == null)
            {
                var result = await WebView.ExecuteScriptAsync("getToken();");

                if (result != null && result != null && result.StartsWith('"') && result.EndsWith('"'))
                {
                    var token = Regex.Unescape(result);
                    _token = token.Substring(1, token.Length - 2);
                }
            }

            return _token;
        }

    }
}
