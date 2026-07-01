using System;
using System.IO;
using System.Threading.Tasks;
using LTC2.Desktopclients.MauiClient.Controls;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;

namespace LTC2.Desktopclients.MauiClient.Platforms.Windows
{
    public partial class Ltc2WebViewHandler : ViewHandler<Ltc2WebView, WebView2>
    {
        public static PropertyMapper<Ltc2WebView, Ltc2WebViewHandler> Mapper = new PropertyMapper<Ltc2WebView, Ltc2WebViewHandler>(ViewHandler.ViewMapper);

        public Ltc2WebViewHandler() : base(Mapper)
        {
        }

        protected override WebView2 CreatePlatformView()
        {
            return new WebView2();
        }

        protected override void ConnectHandler(WebView2 platformView)
        {
            base.ConnectHandler(platformView);

            platformView.CoreWebView2Initialized += OnCoreWebView2Initialized;
            platformView.WebMessageReceived += OnWebMessageReceived;
            platformView.NavigationStarting += OnNavigationStarting;
            platformView.NavigationCompleted += OnNavigationCompleted;
        }

        protected override void DisconnectHandler(WebView2 platformView)
        {
            platformView.CoreWebView2Initialized -= OnCoreWebView2Initialized;
            platformView.WebMessageReceived -= OnWebMessageReceived;
            platformView.NavigationStarting -= OnNavigationStarting;
            platformView.NavigationCompleted -= OnNavigationCompleted;
            base.DisconnectHandler(platformView);
        }

        private async void OnCoreWebView2Initialized(WebView2 sender, Microsoft.UI.Xaml.Controls.CoreWebView2InitializedEventArgs args)
        {
            var webviewRoot = VirtualView?.WebviewRoot;
            if (!string.IsNullOrEmpty(webviewRoot) && !Directory.Exists(webviewRoot))
            {
                Directory.CreateDirectory(webviewRoot);
            }

            VirtualView?.RaiseCoreWebView2Initialized();
        }

        private void OnWebMessageReceived(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs args)
        {
            try
            {
                var message = args.TryGetWebMessageAsString();
                MainThread.BeginInvokeOnMainThread(() => VirtualView?.RaiseWebMessageReceived(message));
            }
            catch
            {
                // ignore
            }
        }

        private void OnNavigationStarting(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
        {
            MainThread.BeginInvokeOnMainThread(() => VirtualView?.RaiseNavigationStarting());
        }

        private void OnNavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            var url = sender.Source?.ToString();
            MainThread.BeginInvokeOnMainThread(() => VirtualView?.RaiseNavigationCompleted(url));
        }

        public async Task NavigateAsync(string url)
        {
            await PlatformView.EnsureCoreWebView2Async();
            PlatformView.Source = new Uri(url);
        }

        public async Task EnsureInitializedAsync()
        {
            await PlatformView.EnsureCoreWebView2Async();
        }

        public async Task<string> EvaluateJavaScriptAsync(string script)
        {
            await PlatformView.EnsureCoreWebView2Async();
            return await PlatformView.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async Task<string> GetCookieAsync(string cookieName, string url)
        {
            await PlatformView.EnsureCoreWebView2Async();
            var cookies = await PlatformView.CoreWebView2.CookieManager.GetCookiesAsync(url);
            foreach (var cookie in cookies)
            {
                if (cookie.Name == cookieName)
                    return cookie.Value;
            }
            return null;
        }

        public async Task DeleteCookiesAsync(System.Collections.Generic.List<string> skipCookies = null)
        {
            await PlatformView.EnsureCoreWebView2Async();
            var cookieManager = PlatformView.CoreWebView2.CookieManager;

            var allCookies = await cookieManager.GetCookiesAsync(null);
            foreach (var cookie in allCookies)
            {
                if (cookie.Domain.Contains("www.strava.com") || cookie.Domain.Contains("ridewithgps.com"))
                {
                    if (skipCookies != null && skipCookies.Contains(cookie.Name))
                        continue;
                    cookieManager.DeleteCookie(cookie);
                }
            }
        }

        public async Task AddScriptToExecuteOnDocumentCreatedAsync(string script)
        {
            await PlatformView.EnsureCoreWebView2Async();
            await PlatformView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(script);
        }

        public void SetUserDataFolder(string folder)
        {
        }
    }
}
