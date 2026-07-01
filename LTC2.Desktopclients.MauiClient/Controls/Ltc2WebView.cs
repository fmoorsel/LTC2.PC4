using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

#if WINDOWS
using LTC2.Desktopclients.MauiClient.Platforms.Windows;
#endif

namespace LTC2.Desktopclients.MauiClient.Controls
{
    public class Ltc2WebView : View
    {
        public event EventHandler<string> WebMessageReceived;
        public event EventHandler NavigationStarting;
        public event EventHandler<string> NavigationCompleted;
        public event EventHandler CoreWebView2Initialized;

        public string WebviewRoot { get; set; }

        public void RaiseWebMessageReceived(string message) => WebMessageReceived?.Invoke(this, message);
        public void RaiseNavigationStarting() => NavigationStarting?.Invoke(this, EventArgs.Empty);
        public void RaiseNavigationCompleted(string url) => NavigationCompleted?.Invoke(this, url);
        public void RaiseCoreWebView2Initialized() => CoreWebView2Initialized?.Invoke(this, EventArgs.Empty);

        public async Task NavigateAsync(string url)
        {
#if WINDOWS
            if (Handler is Ltc2WebViewHandler h) await h.NavigateAsync(url);
#endif
        }

        public async Task EnsureInitializedAsync()
        {
#if WINDOWS
            if (Handler is Ltc2WebViewHandler h) await h.EnsureInitializedAsync();
#endif
        }

        public async Task<string> EvaluateJavaScriptAsync(string script)
        {
#if WINDOWS
            if (Handler is Ltc2WebViewHandler h) return await h.EvaluateJavaScriptAsync(script);
#endif
            return null;
        }

        public async Task<string> GetCookieAsync(string cookieName, string url)
        {
#if WINDOWS
            if (Handler is Ltc2WebViewHandler h) return await h.GetCookieAsync(cookieName, url);
#endif
            return null;
        }

        public async Task DeleteCookiesAsync(System.Collections.Generic.List<string> skipCookies = null)
        {
#if WINDOWS
            if (Handler is Ltc2WebViewHandler h) await h.DeleteCookiesAsync(skipCookies);
#endif
        }

        public async Task AddScriptToExecuteOnDocumentCreatedAsync(string script)
        {
#if WINDOWS
            if (Handler is Ltc2WebViewHandler h) await h.AddScriptToExecuteOnDocumentCreatedAsync(script);
#endif
        }
    }
}
