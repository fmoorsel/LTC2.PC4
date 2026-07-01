using LTC2.Desktopclients.MauiProfileManager.Controls;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace LTC2.Desktopclients.MauiProfileManager.Platforms.Windows
{
    public partial class Ltc2WebViewHandler : ViewHandler<Ltc2WebView, WebView2>
    {
        public static PropertyMapper<Ltc2WebView, Ltc2WebViewHandler> Mapper =
            new PropertyMapper<Ltc2WebView, Ltc2WebViewHandler>(ViewHandler.ViewMapper);

        public Ltc2WebViewHandler() : base(Mapper) { }

        protected override WebView2 CreatePlatformView() => new WebView2();

        protected override void ConnectHandler(WebView2 platformView)
        {
            base.ConnectHandler(platformView);
            platformView.WebMessageReceived += OnWebMessageReceived;
            platformView.NavigationStarting += OnNavigationStarting;
            platformView.NavigationCompleted += OnNavigationCompleted;
        }

        protected override void DisconnectHandler(WebView2 platformView)
        {
            platformView.WebMessageReceived -= OnWebMessageReceived;
            platformView.NavigationStarting -= OnNavigationStarting;
            platformView.NavigationCompleted -= OnNavigationCompleted;
            base.DisconnectHandler(platformView);
        }

        private void OnWebMessageReceived(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs args)
        {
            try
            {
                var message = args.TryGetWebMessageAsString();
                MainThread.BeginInvokeOnMainThread(() => VirtualView?.RaiseWebMessageReceived(message));
            }
            catch { }
        }

        private void OnNavigationStarting(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
        {
            MainThread.BeginInvokeOnMainThread(() => VirtualView?.RaiseNavigationStarting());
        }

        private void OnNavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            MainThread.BeginInvokeOnMainThread(() => VirtualView?.RaiseNavigationCompleted());
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

        public async Task DeleteCookiesAsync(string[] domains)
        {
            await PlatformView.EnsureCoreWebView2Async();
            var cookieManager = PlatformView.CoreWebView2.CookieManager;
            var cookies = await cookieManager.GetCookiesAsync(null);
            foreach (var cookie in cookies)
            {
                foreach (var domain in domains)
                {
                    if (cookie.Domain.Contains(domain))
                    {
                        cookieManager.DeleteCookie(cookie);
                        break;
                    }
                }
            }
        }
    }
}
