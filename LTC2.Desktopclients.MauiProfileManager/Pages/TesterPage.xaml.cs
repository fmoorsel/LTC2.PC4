using LTC2.Desktopclients.MauiProfileManager.Models;
using LTC2.Shared.BaseMessages.Interfaces;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using System;
using System.Threading.Tasks;

namespace LTC2.Desktopclients.MauiProfileManager.Pages
{
    public partial class TesterPage : ContentPage
    {
        public bool IsTestSuccessFull { get; private set; }
        public string ProfileToTest { get; set; }
        public string AthleteId { get; private set; }
        public string Source { get; set; }

        private readonly AppSettings _appSettings;
        private readonly IBaseTranslationService _translationService;

        private IDispatcherTimer _timer;
        private bool _isInitialized;

        public TesterPage(AppSettings appSettings, IBaseTranslationService translationService)
        {
            _appSettings = appSettings;
            _translationService = translationService;

            InitializeComponent();

            BtnLink.Text = _translationService.GetMessage("button.link.profile");
            Title = _translationService.GetMessage("window.tester.profiles");

            WebView.NavigationStarting += (s, e) => MainThread.InvokeOnMainThreadAsync(() => EnableDisableSpinner(true));
            WebView.NavigationCompleted += (s, e) => MainThread.InvokeOnMainThreadAsync(() => EnableDisableSpinner(false));

            _timer = Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += OnTimer;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!_isInitialized)
            {
                _isInitialized = true;
                await InitializeWebView();
            }
        }

        private async Task InitializeWebView()
        {
            try
            {
                if (!string.IsNullOrEmpty(_appSettings.WebviewRoot) &&
                    !System.IO.Directory.Exists(_appSettings.WebviewRoot))
                {
                    System.IO.Directory.CreateDirectory(_appSettings.WebviewRoot);
                }

                WebView.UserDataFolder = _appSettings.WebviewRoot;
                await WebView.EnsureInitializedAsync();
                await WebView.DeleteCookiesAsync(new[] { "strava.com", "ridewithgps.com" });
                await WebView.NavigateAsync(GetUrl());
                _timer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TesterPage.InitializeWebView error: {ex.Message}");
            }
        }

        private void EnableDisableSpinner(bool enable)
        {
            CtrProgressRing.IsRunning = enable;
            CtrProgressRing.IsVisible = enable;
        }

        private async void OnTimer(object sender, EventArgs e)
        {
            try
            {
                var result = await WebView.EvaluateJavaScriptAsync("AthleteId()");
                var success = result != null && result != "null";
                if (success)
                {
                    var athleteId = result.Trim('"');
                    if (!string.IsNullOrEmpty(athleteId))
                    {
                        AthleteId = athleteId;
                        IsTestSuccessFull = true;
                        await MainThread.InvokeOnMainThreadAsync(() => BtnLink.IsEnabled = true);
                    }
                }
            }
            catch { /* JS not ready yet */ }
        }

        private void OnClickHandlerLink(object sender, EventArgs e)
        {
            _timer.Stop();
            IsTestSuccessFull = !string.IsNullOrEmpty(AthleteId);
            Navigation.PopModalAsync();
        }

        private string GetUrl()
        {
            return $"{_appSettings.StartPage}?profile={ProfileToTest}&language={_translationService.CurrentLanguage}&source={Source}";
        }
    }
}
