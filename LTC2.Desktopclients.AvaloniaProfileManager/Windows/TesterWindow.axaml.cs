using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Threading;
using AvaloniaProgressRing;
using LTC2.Desktopclients.AvaloniaProfileManager.Models;
using LTC2.Shared.BaseMessages.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LTC2.Desktopclients.AvaloniaProfileManager.Windows
{
    public partial class TesterWindow : Window
    {
        public bool IsTestSuccessFull { get; private set; }
        public string ProfileToTest { get; set; }
        public string AthleteId { get; private set; }
        public string Source { get; set; }

        private readonly AppSettings _appSettings;
        private readonly IBaseTranslationService _translationService;

        private NativeWebView _webView;
        private ProgressRing _progressRing;
        private Button _btnLink;

        private bool _isAdapterCreated;
        private DispatcherTimer _timer;

        public TesterWindow()
        {
        }

        public TesterWindow(AppSettings appSettings, IBaseTranslationService translationService)
        {
            _appSettings = appSettings;
            _translationService = translationService;

            InitializeComponent();
            InitControls();
        }

        private void InitControls()
        {
            _webView = this.FindControl<NativeWebView>("WebView");
            _progressRing = this.FindControl<ProgressRing>("CtrProgressRing");
            _btnLink = this.FindControl<Button>("BtnLink");

            if (_webView != null)
            {
                _webView.EnvironmentRequested += OnEnvironmentRequested;
                _webView.AdapterCreated += OnAdapterCreated;
                _webView.NavigationStarted += OnNavigationStarted;
                _webView.NavigationCompleted += OnNavigationCompleted;
            }

            TryTranslate(_btnLink, "button.link.profile");
            TryTranslate(this, "window.tester.profiles");

            _timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(500) };
            _timer.Tick += OnTimer;
        }

        private void TryTranslate(Control control, string key)
        {
            if (control == null) return;
            var msg = _translationService.GetMessage(key);
            if (control is Button btn) btn.Content = msg;
            else if (control is Window wnd) wnd.Title = msg;
        }

        public async Task TestProfile(Window parent)
        {
            IsTestSuccessFull = false;
            AthleteId = null;

            if (_btnLink != null) _btnLink.IsEnabled = false;

            await this.ShowDialog(parent);
        }

        private void OnEnvironmentRequested(object sender, WebViewEnvironmentRequestedEventArgs args)
        {
            if (args is WindowsWebView2EnvironmentRequestedEventArgs webView2)
            {
                if (!Directory.Exists(_appSettings.WebviewRoot))
                {
                    Directory.CreateDirectory(_appSettings.WebviewRoot);
                }

                webView2.UserDataFolder = _appSettings.WebviewRoot;
            }
            else if (args is LinuxWpeWebViewEnvironmentRequestedEventArgs wpe)
            {
                wpe.PreferWebKitGtkInstead = true;
            }
        }

        private async void OnAdapterCreated(object sender, EventArgs e)
        {
            if (_isAdapterCreated) return;
            _isAdapterCreated = true;

            try
            {
                await DeleteCookies();

                _webView.Source = new Uri(GetUrl());

                _timer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TesterWindow.OnAdapterCreated error: {ex.Message}");
            }
        }

        private async Task DeleteCookies()
        {
            var cookieManager = _webView?.TryGetCookieManager();

            if (cookieManager == null) return;

            try
            {
                var cookies = await cookieManager.GetCookiesAsync();

                foreach (var cookie in cookies)
                {
                    if (cookie.Domain.Contains("strava.com") || cookie.Domain.Contains("ridewithgps.com"))
                    {
                        cookieManager.DeleteCookie(cookie.Name, cookie.Domain, cookie.Path);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting cookies: {ex.Message}");
            }
        }

        private void OnNavigationStarted(object sender, WebViewNavigationStartingEventArgs e)
        {
            Dispatcher.UIThread.Post(() => EnableDisableSpinner(true));
        }

        private void OnNavigationCompleted(object sender, WebViewNavigationCompletedEventArgs e)
        {
            Dispatcher.UIThread.Post(() => EnableDisableSpinner(false));
        }

        private void EnableDisableSpinner(bool enable)
        {
            if (_progressRing != null)
            {
                _progressRing.IsActive = enable;
                _progressRing.IsVisible = enable;
            }
        }

        private async void OnTimer(object sender, EventArgs e)
        {
            try
            {
                var result = await _webView.InvokeScript("AthleteId()");
                var success = result != null && result != "null";

                if (success)
                {
                    var athleteId = result.Trim('"');

                    if (!string.IsNullOrEmpty(athleteId))
                    {
                        AthleteId = athleteId;
                        IsTestSuccessFull = true;

                        Dispatcher.UIThread.Post(() =>
                        {
                            if (_btnLink != null) _btnLink.IsEnabled = true;
                        });
                    }
                }
            }
            catch
            {
                // ignore - JS not ready yet
            }
        }

        public void ClickHandlerLink(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            IsTestSuccessFull = !string.IsNullOrEmpty(AthleteId);
            Close();
        }

        private string GetUrl()
        {
            return $"{_appSettings.StartPage}?profile={ProfileToTest}&language={_translationService.CurrentLanguage}&source={Source}";
        }
    }
}
