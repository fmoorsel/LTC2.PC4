using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace LTC2.Desktopclients.MauiProfileManager.Controls
{
    public class Ltc2WebView : View
    {
        public event EventHandler<string> WebMessageReceived;
        public event EventHandler NavigationStarting;
        public event EventHandler NavigationCompleted;

        public string UserDataFolder { get; set; }

        internal void RaiseWebMessageReceived(string msg) => WebMessageReceived?.Invoke(this, msg);
        internal void RaiseNavigationStarting() => NavigationStarting?.Invoke(this, EventArgs.Empty);
        internal void RaiseNavigationCompleted() => NavigationCompleted?.Invoke(this, EventArgs.Empty);

        public Task<string> EvaluateJavaScriptAsync(string script)
        {
#if WINDOWS
            var handler = Handler as LTC2.Desktopclients.MauiProfileManager.Platforms.Windows.Ltc2WebViewHandler;
            return handler?.EvaluateJavaScriptAsync(script) ?? Task.FromResult<string>(null);
#else
            return Task.FromResult<string>(null);
#endif
        }

        public Task NavigateAsync(string url)
        {
#if WINDOWS
            var handler = Handler as LTC2.Desktopclients.MauiProfileManager.Platforms.Windows.Ltc2WebViewHandler;
            return handler?.NavigateAsync(url) ?? Task.CompletedTask;
#else
            return Task.CompletedTask;
#endif
        }

        public Task EnsureInitializedAsync()
        {
#if WINDOWS
            var handler = Handler as LTC2.Desktopclients.MauiProfileManager.Platforms.Windows.Ltc2WebViewHandler;
            return handler?.EnsureInitializedAsync() ?? Task.CompletedTask;
#else
            return Task.CompletedTask;
#endif
        }

        public Task DeleteCookiesAsync(string[] domains)
        {
#if WINDOWS
            var handler = Handler as LTC2.Desktopclients.MauiProfileManager.Platforms.Windows.Ltc2WebViewHandler;
            return handler?.DeleteCookiesAsync(domains) ?? Task.CompletedTask;
#else
            return Task.CompletedTask;
#endif
        }
    }
}
