using LTC2.Desktopclients.MauiClient.Models;
using LTC2.Desktopclients.MauiClient.Services;
using LTC2.Desktopclients.MauiClient.Windows;
using LTC2.Shared.BaseMessages.Interfaces;
using LTC2.Shared.Models.Interprocess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LTC2.Desktopclients.MauiClient.Pages
{
    public partial class BrowserPage : ContentPage
    {
        private bool _isClosing;
        private bool _isUpdating;
        private bool _inFatalMode;
        private bool _isLoaded;

        private readonly IServiceProvider _services;
        private readonly AppSettings _appSettings;
        private readonly StatusNotifier _statusNotifier;
        private readonly WebViewConnector _webViewConnector;
        private readonly ProfileManager _profileManager;
        private readonly MultiSportManager _multiSportManager;
        private readonly IBaseTranslationService _translationService;

        private readonly List<string> _refreshEnabledFor;
        private IDispatcherTimer _timer;
        private RoutePlannerWindow _routePlannerWindow;

        public BrowserPage(
            IServiceProvider services,
            AppSettings appSettings,
            StatusNotifier statusNotifier,
            WebViewConnector webViewConnector,
            ProfileManager profileManager,
            MultiSportManager multiSportManager,
            IBaseTranslationService translationService)
        {
            _services = services;
            _appSettings = appSettings;
            _statusNotifier = statusNotifier;
            _webViewConnector = webViewConnector;
            _profileManager = profileManager;
            _multiSportManager = multiSportManager;
            _translationService = translationService;

            _refreshEnabledFor = appSettings?.EnableRefreshFor?.Split(',').ToList() ?? new List<string>();

            InitializeComponent();

            Title = _translationService.GetMessage("title.browser.window");
            BtnUpdate.Text = _translationService.GetMessage("button.browser.update");

            WebView.WebMessageReceived += OnWebMessageReceived;
            WebView.NavigationStarting += (s, e) => MainThread.InvokeOnMainThreadAsync(() => EnableDisableSpinner(true));
            WebView.NavigationCompleted += (s, url) => OnNavigationCompleted(s, url);

            _webViewConnector.WebView = WebView;

            _timer = Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += OnTimer;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!_isLoaded)
            {
                _isLoaded = true;
                _statusNotifier.OnStatusNotification += OnStatusNotification;

                try
                {
                    if (!string.IsNullOrEmpty(_appSettings.WebviewRoot) && !Directory.Exists(_appSettings.WebviewRoot))
                        Directory.CreateDirectory(_appSettings.WebviewRoot);

                    WebView.WebviewRoot = _appSettings.WebviewRoot;
                    await WebView.EnsureInitializedAsync();

                    if (_profileManager.HasMultipleProfiles)
                        await WebView.DeleteCookiesAsync(new System.Collections.Generic.List<string> { "www.strava.com", "ridewithgps.com" });

                    await WebView.NavigateAsync(GetUrl());

                    var prefix = _multiSportManager.RunInMultiSportMode ? "MultiSport - " : string.Empty;
                    Title = prefix + _translationService.GetMessage("title.browser.window");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"BrowserPage init error: {ex.Message}");
                }

                _timer.Start();
            }
        }

        private void OnTimer(object sender, EventArgs e)
        {
            if (!_inFatalMode) _statusNotifier.CheckKeepAliveStatuses();
        }

        private void EnableDisableSpinner(bool enable)
        {
            CtrProgressRing.IsRunning = enable;
            CtrProgressRing.IsVisible = enable;
        }

        private void OnWebMessageReceived(object sender, string message)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    var webMessage = JsonConvert.DeserializeObject<GenericWebMessage>(message);
                    if (webMessage?.Message == "routeplanner")
                    {
                        MainThread.InvokeOnMainThreadAsync(() => ShowRoutePlanner());
                    }
                }
            }
            catch { }
        }

        private void ShowRoutePlanner()
        {
            if (_routePlannerWindow == null)
            {
                var page = _services.GetRequiredService<RoutePlannerPage>();
                _routePlannerWindow = new RoutePlannerWindow(page);
                Application.Current.OpenWindow(_routePlannerWindow);
            }
            else
            {
                Application.Current.ActivateWindow(_routePlannerWindow);
            }
        }

        private async void OnNavigationCompleted(object sender, string url)
        {
            MainThread.InvokeOnMainThreadAsync(() => EnableDisableSpinner(false));

            _ = _webViewConnector.Login();
        }

        private void OnStatusNotification(object sender, OnStatusMessageEventArguments e)
        {
            MainThread.InvokeOnMainThreadAsync(() => UpdateStatus(e.Status));
        }

        private void UpdateStatus(StatusMessage status)
        {
            if (_isClosing) return;

            var component = GetComponentNiceName(status.Origin);

            if (status.Status == StatusMessage.STATUS_PING)
            {
                var msg = GetDateFromPing(status.Message);
                var labelText = $"{component} {msg}";
                if (status.Origin == StatusMessage.ORG_CALCULATOR) LblPingCalculator.Text = labelText;
                else if (status.Origin == StatusMessage.ORG_WEBAPP) LblPingMainApp.Text = labelText;
            }
            else if (status.Status == StatusMessage.STATUS_STARTUPDATE) _isUpdating = true;
            else if (status.Status == StatusMessage.STATUS_ENDUPDATE) _isUpdating = false;
            else if (status.Status == StatusMessage.STATUS_CHECK)
            {
                var msgParts = status.Message.Split(' ');
                var msg = _translationService.GetMessage("label.progress.check.1", msgParts[0]);
                if (msgParts.Length >= 2) msg = _translationService.GetMessage("label.progress.check.2", msgParts.ToList());
                LblStatusUpdate.Text = msg;
            }
            else if (status.Status == StatusMessage.STATUS_RESULT)
            {
                LblStatusUpdate.Text = string.Empty;
                _multiSportManager.RefreshCurrentActivityTypes();
                _ = WebView.NavigateAsync(GetUrl());
            }
            else if (status.Status == StatusMessage.STATUS_LIMIT)
            {
                LblStatusUpdate.Text = _translationService.GetMessage("progress.limit");
                _ = WebView.NavigateAsync(GetUrl());
            }
            else if (status.Status == StatusMessage.STATUS_WAIT)
            {
                LblStatusUpdate.Text = $" {_translationService.GetMessage("progress.quater.limit")} {status.Message}";
            }
            else if (status.Status == StatusMessage.STATUS_FATAL && !_inFatalMode)
            {
                _inFatalMode = true;
                _ = HandleFatal(component);
            }
        }

        private async Task HandleFatal(string component)
        {
            var text = _translationService.GetMessage("messagebox.fatal.component.exit", component);
            var caption = _translationService.GetMessage("messagebox.fatal.component.exit.header");
            await DisplayAlert(caption, text, "OK");
            Environment.Exit(0);
        }

        private async void OnUpdate(object sender, EventArgs e)
        {
            var updatePage = _services.GetRequiredService<UpdatePage>();
            await Navigation.PushModalAsync(updatePage);
        }

        private string GetUrl()
        {
            return $"{_appSettings.StartPage}?language={_translationService.CurrentLanguage}&id={Guid.NewGuid()}&multi={_multiSportManager.RunInMultiSportMode}&source={_multiSportManager.RunWithSource}";
        }

        private string GetDateFromPing(string msg)
        {
            if (msg == null) return string.Empty;
            try { return DateTime.SpecifyKind(DateTime.Parse(msg), DateTimeKind.Utc).ToLocalTime().ToString(CultureInfo.InvariantCulture); }
            catch { return string.Empty; }
        }

        private string GetComponentNiceName(string component)
        {
            if (component == StatusMessage.ORG_CALCULATOR) return _translationService.GetMessage($"component.{StatusMessage.ORG_CALCULATOR}");
            if (component == StatusMessage.ORG_WEBAPP) return _translationService.GetMessage($"component.{StatusMessage.ORG_WEBAPP}");
            return string.Empty;
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            if (!_isClosing)
            {
                if (_isUpdating)
                {
                    var text = _translationService.GetMessage("messagebox.exit.confirm");
                    var caption = _translationService.GetMessage("messagebox.exit.confirm.header");
                    var answer = await DisplayAlert(caption, text, "Ja", "Nee");
                    if (answer) Environment.Exit(0);
                }
                else
                {
                    _isClosing = true;
                    Environment.Exit(0);
                }
            }
        }
    }
}
