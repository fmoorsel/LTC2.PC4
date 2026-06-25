using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaProgressRing;
using LTC2.Desktopclients.AvaloniaClient.Models;
using LTC2.Desktopclients.AvaloniaClient.Services;
using LTC2.Shared.BaseMessages.Interfaces;
using LTC2.Shared.Models.Interprocess;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LTC2.Desktopclients.AvaloniaClient.Windows
{
    public partial class BrowserWindow : Window
    {
        private bool _isClosing;
        private bool _isUpdating;
        private bool _inFatalMode;

        private NativeWebView _webView;
        private ProgressRing _progressRing;
        private Button _btnUpdate;
        private TextBlock _txtBtnUpdate;
        private DispatcherTimer _timer;

        private TextBlock _lblStatusCalculator;
        private TextBlock _lblStatusMainApp;
        private TextBlock _lblUpdateProgress;

        private readonly UpdateWindow _updateWindow;
        private readonly IBaseTranslationService _translationService;
        private readonly StatusNotifier _statusNotifier;
        private readonly WebViewConnector _webViewConnector;
        private readonly ApplicationManager _applicationManager;
        private readonly ProfileManager _profileManager;
        private readonly MultiSportManager _multiSportManager;
        private readonly AppSettings _appSettings;
        private readonly List<string> _refreshEnabledFor;

        private bool _isLoaded;

        public BrowserWindow()
        {
        }

        public BrowserWindow(
            AppSettings appSettings,
            ApplicationManager applicationManager,
            StatusNotifier statusNotifier,
            UpdateWindow updateWindow,
            WebViewConnector webViewConnector,
            ProfileManager profileManager,
            MultiSportManager multiSportManager,
            IBaseTranslationService translationService) : this()
        {
            _translationService = translationService;
            _statusNotifier = statusNotifier;
            _webViewConnector = webViewConnector;
            _updateWindow = updateWindow;
            _applicationManager = applicationManager;
            _profileManager = profileManager;
            _multiSportManager = multiSportManager;
            _appSettings = appSettings;

            _refreshEnabledFor = appSettings?.EnableRefreshFor?.Split(',').ToList() ?? new List<string>();

            InitializeComponent();

            this.Closing += OnClose;
            this.Activated += OnActivated;

            InitControls();
        }

        private void InitControls()
        {
            _webView = this.FindControl<NativeWebView>("WebView");
            _progressRing = this.FindControl<ProgressRing>("CtrProgressRing");
            _btnUpdate = this.FindControl<Button>("BtnUpdate");
            _txtBtnUpdate = this.FindControl<TextBlock>("TxtBtnUpdate");
            _lblStatusCalculator = this.FindControl<TextBlock>("LblPingCalculator");
            _lblStatusMainApp = this.FindControl<TextBlock>("LblPingMainApp");
            _lblUpdateProgress = this.FindControl<TextBlock>("LblStatusUpdate");

            if (_webView != null)
            {
                _webView.EnvironmentRequested += OnEnvironmentRequested;
                _webView.NavigationStarted += OnBeforeNavigate;
                _webView.NavigationCompleted += OnNavigated;
                _webView.WebMessageReceived += OnWebMessage;
                _webView.AdapterCreated += OnAdapterCreated;

                _webViewConnector.WebView = _webView;
            }

            TryTranslate(_txtBtnUpdate, "button.browser.update");
            TryTranslate(this, "title.browser.window");

            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += OnTimer;
        }


        private void OnEnvironmentRequested(object sender, WebViewEnvironmentRequestedEventArgs args)
        {
            if (args is WindowsWebView2EnvironmentRequestedEventArgs webView2)
            {
                if (!Directory.Exists(_appSettings.WebviewRoot))
                {
                    Directory.CreateDirectory(_appSettings.WebviewRoot);
                }

                webView2.EnableDevTools = true;
                webView2.UserDataFolder = _appSettings.WebviewRoot;
            }
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            _statusNotifier.OnStatusNotification += OnStatusNotification;
        }

        private async void OnAdapterCreated(object sender, EventArgs e)
        {
            try
            {
                if (!_isLoaded)
                {
                    _isLoaded = true;

                    _webView.Source = new Uri(GetUrl());

                    if (_profileManager.HasMultipleProfiles)
                    {
                        await DeleteCookies();
                    }

                    var prefix = _multiSportManager.RunInMultiSportMode ? "MultiSport - " : string.Empty;
                    Title = prefix + Title;
                }
            }
            catch (Exception)
            {
                // ignore, we don't want to crash the app if something goes wrong here
            }
        }


        private async Task DeleteCookies()
        {
            var cookieManager = _webView.TryGetCookieManager();

            if (cookieManager == null)
            {
                return;
            }

            try
            {
                var cookies = await cookieManager.GetCookiesAsync();
                var stravaCookies = cookies.Where(c => c.Domain.Contains("www.strava.com"));

                foreach (var cookie in stravaCookies)
                {
                    var cookieName = cookie.Name;

                    if (_appSettings.SkipCookiesWhileDeleting != null && _appSettings.SkipCookiesWhileDeleting.Contains(cookieName))
                    {
                        continue;
                    }

                    cookieManager.DeleteCookie(cookie.Name, cookie.Domain, cookie.Path);
                }

                var rwgpsCookies = cookies.Where(c => c.Domain.Contains("ridewithgps.com"));

                foreach (var cookie in rwgpsCookies)
                {
                    var cookieName = cookie.Name;

                    if (_appSettings.SkipCookiesWhileDeleting != null && _appSettings.SkipCookiesWhileDeleting.Contains(cookieName))
                    {
                        continue;
                    }

                    cookieManager.DeleteCookie(cookie.Name, cookie.Domain, cookie.Path);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting cookies: {ex.Message}");
            }
        }


        private async void OnWebMessage(object sender, WebMessageReceivedEventArgs args)
        {
            var message = args.Body;

            if (message == "selectFile")
            {
                var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
                {
                    AllowMultiple = false
                });

                if (files != null && files.Count > 0)
                {
                    var file = files[0];
                    var fileName = file.Path.LocalPath;

                    _webViewConnector.FireOnFileEvent(fileName);
                }
            }
        }

        private void OnActivated(object sender, EventArgs e)
        {
            if (!_timer.IsEnabled)
            {
                _timer.Start();
            }
        }

        private void TryTranslate(Control control, string message)
        {
            if (control is TextBlock textBlock)
            {
                textBlock.Text = _translationService.GetMessage(message);
            }

            if (control is Button button)
            {
                button.Content = _translationService.GetMessage(message);
            }

            if (control is Window window)
            {
                window.Title = _translationService.GetMessage(message);
            }
        }

        private void OnBeforeNavigate(object sender, WebViewNavigationStartingEventArgs arg)
        {
            Dispatcher.UIThread.Post(() => EnableDisableSpinner(true));
        }

        private async Task HandleFatal(string component)
        {
            var text = _translationService.GetMessage("messagebox.fatal.component.exit", component);
            var caption = _translationService.GetMessage("messagebox.fatal.component.exit.header");

            var box = MessageBoxManager.GetMessageBoxStandard(caption, text);

            this.WindowState = WindowState.Normal;

            await box.ShowWindowDialogAsync(_applicationManager.MainWindow ?? this);

            Close();
        }

        public void ClickHandlerUpdate(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;

            _updateWindow.Show(this);
        }

        private void EnableDisableSpinner(bool enable)
        {
            if (_progressRing != null)
            {
                _progressRing.IsActive = enable;
                _progressRing.IsVisible = enable;
            }
        }

        private void OnNavigated(object sender, WebViewNavigationCompletedEventArgs arg)
        {
            Dispatcher.UIThread.Post(() => EnableDisableSpinner(false));

            var url = _webView?.Source?.ToString();

            if (_refreshEnabledFor != null && url != null)
            {
                foreach (var startUrl in _refreshEnabledFor)
                {
                    if (url.StartsWith(startUrl))
                    {
                        _btnUpdate.IsEnabled = true;
                        break;
                    }
                }

                _ = _webViewConnector.Login();
            }
        }

        private void SingalStopping()
        {
            Environment.Exit(0);
        }

        private async void OnClose(object sender, WindowClosingEventArgs args)
        {
            if (!_isClosing)
            {
                args.Cancel = true;

                if (_isUpdating)
                {
                    var text = _translationService.GetMessage("messagebox.exit.confirm");
                    var caption = _translationService.GetMessage("messagebox.exit.confirm.header");

                    var box = MessageBoxManager.GetMessageBoxStandard(caption, text, ButtonEnum.YesNo);
                    var answer = await box.ShowWindowDialogAsync(this);

                    if (answer == ButtonResult.Yes)
                    {
                        Environment.Exit(0);
                    }
                }
                else
                {
                    _isClosing = true;
                }

                if (_isClosing)
                {
                    Dispatcher.UIThread.Post(() => SingalStopping());
                }
            }
        }

        private void OnStatusNotification(object sender, OnStatusMessageEventArguments e)
        {
            Dispatcher.UIThread.Post(() => UpdateStatus(e.Status));
        }

        private void UpdateStatus(StatusMessage status)
        {
            if (_isClosing)
            {
                return;
            }

            var component = GetComponentNiceName(status.Origin);

            if (status.Status == StatusMessage.STATUS_PING)
            {
                var msg = GetDateFromPing(status.Message);
                var labelText = $"{component} {msg}";

                if (status.Origin == StatusMessage.ORG_CALCULATOR)
                {
                    _lblStatusCalculator.Text = labelText;
                }
                else if (status.Origin == StatusMessage.ORG_WEBAPP)
                {
                    _lblStatusMainApp.Text = labelText;
                }
            }
            else if (status.Status == StatusMessage.STATUS_STARTUPDATE)
            {
                _isUpdating = true;
            }
            else if (status.Status == StatusMessage.STATUS_ENDUPDATE)
            {
                _isUpdating = false;
            }
            else if (status.Status == StatusMessage.STATUS_CHECK)
            {
                var msgParts = status.Message.Split(' ');
                var msg = _translationService.GetMessage("label.progress.check.1", msgParts[0]);

                if (msgParts.Length >= 2)
                {
                    msg = _translationService.GetMessage("label.progress.check.2", msgParts.ToList());
                }

                _lblUpdateProgress.Text = msg;
            }
            else if (status.Status == StatusMessage.STATUS_RESULT)
            {
                _lblUpdateProgress.Text = string.Empty;

                _multiSportManager.RefreshCurrentActivityTypes();

                _webView.Source = new Uri(GetUrl());
            }
            else if (status.Status == StatusMessage.STATUS_LIMIT)
            {
                _lblUpdateProgress.Text = _translationService.GetMessage("progress.limit");

                _webView.Source = new Uri(GetUrl());
            }
            else if (status.Status == StatusMessage.STATUS_WAIT)
            {
                _lblUpdateProgress.Text = $" {_translationService.GetMessage("progress.quater.limit")} {status.Message}";
            }
            else if (status.Status == StatusMessage.STATUS_FATAL && !_inFatalMode)
            {
                _inFatalMode = true;

                async void Action() => await HandleFatal(component);

                Dispatcher.UIThread.Post(Action);
            }
        }

        private string GetDateFromPing(string msg)
        {
            if (msg == null) return string.Empty;

            try
            {
                return DateTime.SpecifyKind(DateTime.Parse(msg), DateTimeKind.Utc).ToLocalTime().ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private string GetComponentNiceName(string component)
        {
            if (component == StatusMessage.ORG_CALCULATOR)
            {
                return _translationService.GetMessage($"component.{StatusMessage.ORG_CALCULATOR}");
            }
            else if (component == StatusMessage.ORG_WEBAPP)
            {
                return _translationService.GetMessage($"component.{StatusMessage.ORG_WEBAPP}");
            }

            return string.Empty;
        }

        private void OnTimer(object sender, EventArgs e)
        {
            if (!_inFatalMode)
            {
                _statusNotifier.CheckKeepAliveStatuses();
            }
        }

        private string GetUrl()
        {
            return $"{_appSettings.StartPage}?language={_translationService.CurrentLanguage}&id={Guid.NewGuid()}&multi={_multiSportManager.RunInMultiSportMode}&source={_multiSportManager.RunWithSource}";
        }
    }
}
