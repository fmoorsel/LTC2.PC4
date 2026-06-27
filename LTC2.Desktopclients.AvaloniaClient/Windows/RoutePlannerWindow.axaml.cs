using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using AvaloniaProgressRing;
using LTC2.Desktopclients.AvaloniaClient.Models;
using LTC2.Desktopclients.AvaloniaClient.Services;
using LTC2.Desktopclients.AvaloniaClient.Utils;
using LTC2.Shared.BaseMessages.Interfaces;
using LTC2.Shared.Http.Interfaces;
using LTC2.Shared.Models.Requests;
using LTC2.Shared.Models.Responses;
using MsBox.Avalonia;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LTC2.Desktopclients.AvaloniaClient.Windows
{
    public partial class RoutePlannerWindow : Window
    {
        private NativeWebView _webView;
        private ProgressRing _progressRing;
        private CheckBox _chkToggleVisibility;
        private Button _btnCheckRoute;
        private Button _btnUnCheckRoute;
        private TextBlock _lblCurrentPlace;

        private bool _isAdapterCreated;
        private bool _oldUncheckedState;
        private WindowState _previousWindowState;

        private string _rawInitScript;
        private string _initScript;

        private readonly object _profileLock = new object();
        private GetProfileResponse _profile;

        private readonly AppSettings _appSettings;
        private readonly MultiSportManager _multiSportManager;
        private readonly IBaseTranslationService _translationService;
        private readonly ILTC2HttpProxy _ltc2Proxy;
        private readonly WebViewConnector _webViewConnector;

        public RoutePlannerWindow()
        {
        }

        public RoutePlannerWindow(
            AppSettings appSettings,
            MultiSportManager multiSportManager,
            IBaseTranslationService translationService,
            WebViewConnector webViewConnector,
            ILTC2HttpProxy ltc2Proxy) : this()
        {
            _appSettings = appSettings;
            _multiSportManager = multiSportManager;
            _translationService = translationService;
            _webViewConnector = webViewConnector;
            _ltc2Proxy = ltc2Proxy;

            _previousWindowState = WindowState.Normal;

            InitializeComponent();

            this.Closing += OnClosing;

            InitControls();
        }

        private void InitControls()
        {
            _webView = this.FindControl<NativeWebView>("WebView");
            _progressRing = this.FindControl<ProgressRing>("CtrProgressRing");
            _chkToggleVisibility = this.FindControl<CheckBox>("ChkToggleVisibility");
            _btnCheckRoute = this.FindControl<Button>("BtnCheckRoute");
            _btnUnCheckRoute = this.FindControl<Button>("BtnUnCheckRoute");
            _lblCurrentPlace = this.FindControl<TextBlock>("LblCurrentPlace");

            if (_webView != null)
            {
                _webView.EnvironmentRequested += OnEnvironmentRequested;
                _webView.AdapterCreated += OnAdapterCreated;
                _webView.NavigationStarted += OnNavigationStarted;
                _webView.NavigationCompleted += OnNavigationCompleted;
                _webView.WebMessageReceived += OnWebMessageReceived;
                _webView.NewWindowRequested += OnNewWindowRequested;
            }

            if (_chkToggleVisibility != null)
            {
                _chkToggleVisibility.IsCheckedChanged += OnToggleVisibilityChanged;
            }

            TryTranslate(this, "title.routeplanner.window");
            TryTranslate(_btnCheckRoute, "button.checkroute");
            TryTranslate(_btnUnCheckRoute, "button.uncheckroute");
            TryTranslate(_chkToggleVisibility, "check.toggle.visibility");
        }

        private void TryTranslate(Control control, string key)
        {
            var text = _translationService?.GetMessage(key);
            if (string.IsNullOrEmpty(text)) return;

            if (control is TextBlock tb) tb.Text = text;
            if (control is Button btn) btn.Content = text;
            if (control is CheckBox cb) cb.Content = text;
            if (control is Window wnd) wnd.Title = text;
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
        }

        private void OnAdapterCreated(object sender, EventArgs e)
        {
            _isAdapterCreated = true;

            _rawInitScript = GetInitScript();

            _webView.Source = new Uri("about:blank", UriKind.Absolute);

            ShowPlanner();
        }

        private static void OnNewWindowRequested(object sender, WebViewNewWindowRequestedEventArgs e)
        {
            e.Handled = true;
        }

        private void OnNavigationStarted(object sender, WebViewNavigationStartingEventArgs args)
        {
            Dispatcher.UIThread.Post(() => EnableDisableSpinner(true));
        }

        private void OnNavigationCompleted(object sender, WebViewNavigationCompletedEventArgs args)
        {
            Dispatcher.UIThread.Post(() => EnableDisableSpinner(false));

            var url = _webView?.Source?.ToString() ?? string.Empty;

            if (!string.IsNullOrEmpty(url) && url.StartsWith(GetStartUrl()) && GetProfile() == null)
            {
                UpdateProfile();
            }

            if (IsBuilderUrl(url))
            {
                _ = InitBuilderAsync();

                Dispatcher.UIThread.Post(() =>
                {
                    _chkToggleVisibility.IsVisible = true;
                    _btnCheckRoute.IsVisible = true;
                    _btnUnCheckRoute.IsVisible = true;
                    _btnUnCheckRoute.IsEnabled = false;
                    _oldUncheckedState = false;
                });
            }
            else
            {
                Dispatcher.UIThread.Post(() =>
                {
                    _lblCurrentPlace.Text = string.Empty;
                    _chkToggleVisibility.IsVisible = false;
                    _btnCheckRoute.IsVisible = false;
                    _btnUnCheckRoute.IsVisible = false;
                    _btnUnCheckRoute.IsEnabled = false;
                    _oldUncheckedState = false;
                });
            }
        }

        private async Task InitBuilderAsync()
        {
            var profile = await EnsureProfile();
            if (profile == null) return;

            string result = null;
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    result = await _webView.InvokeScript(_initScript);
                }
                catch
                {
                    result = null;
                }

                if (result?.Trim('"') == "1") return;

                await Task.Delay(200);
                profile = await EnsureProfile();
                if (profile == null) return;
            }

            if (result?.Trim('"') != "1")
            {
                var msg = _translationService.GetMessage("plugin.install.failed");
                var header = _translationService.GetMessage("plugin.install.failed.header");

                Dispatcher.UIThread.Post(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard(header, msg);
                    await box.ShowWindowDialogAsync(this);
                });
            }
        }

        private void OnWebMessageReceived(object sender, WebMessageReceivedEventArgs e)
        {
            var parameter = e.Body;

            if (parameter.StartsWith("not supported"))
            {
                Dispatcher.UIThread.Post(() =>
                    _lblCurrentPlace.Text = _translationService.GetMessage("routeplanner.no_map_support"));
                return;
            }

            var parts = parameter.Split(',');
            if (parts.Length < 2) return;

            var postcode = parts[0];
            var id = parts[1].Split(':')[0];

            var profile = GetProfile();

            string labelText;

            if (profile != null)
            {
                var visitedAlltime = profile.PlacesInAllTimeScore.FirstOrDefault(p => p.Id == id);
                var visitedYear = profile.PlacesInYearScore.FirstOrDefault(p => p.Id == id);

                if (visitedAlltime != null && visitedYear != null)
                {
                    labelText = _translationService.GetMessage("routeplanner.postcode.year", postcode);
                }
                else if (visitedAlltime != null)
                {
                    labelText = _translationService.GetMessage("routeplanner.postcode.visited", postcode);
                }
                else
                {
                    labelText = _translationService.GetMessage("routeplanner.postcode", postcode);
                }
            }
            else
            {
                labelText = _translationService.GetMessage("routeplanner.postcode", postcode);
            }

            Dispatcher.UIThread.Post(() => _lblCurrentPlace.Text = labelText);
        }

        public void ShowPlanner()
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = _previousWindowState;
            }

            Show();
            Activate();

            if (!_isAdapterCreated) return;

            var currentSource = _webView.Source?.ToString() ?? string.Empty;

            if (!currentSource.StartsWith(GetSitePrefix()))
            {
                _lblCurrentPlace.Text = string.Empty;
                _webView.Source = new Uri(GetStartUrl());
            }
            else if (IsBuilderUrl(currentSource))
            {
                _lblCurrentPlace.Text = "Postcode: ----";
            }
            else
            {
                _lblCurrentPlace.Text = string.Empty;
            }
        }

        private GetProfileResponse GetProfile()
        {
            lock (_profileLock)
            {
                return _profile;
            }
        }

        private async Task<GetProfileResponse> EnsureProfile()
        {
            if (GetProfile() == null)
            {
                SetProfile(await RetrieveProfile());
            }

            return GetProfile();
        }

        private void SetProfile(GetProfileResponse profile)
        {
            lock (_profileLock)
            {
                _profile = profile;
                _initScript = CreateScript(profile);
            }
        }

        private async Task<GetProfileResponse> RetrieveProfile()
        {
            var token = await _webViewConnector.Login();
            return await _ltc2Proxy.GetProfile(token, _multiSportManager.RunInMultiSportMode);
        }

        public void UpdateProfile()
        {
            _ = Task.Run(async () =>
            {
                var profile = await RetrieveProfile();
                SetProfile(profile);
            });
        }

        private string CreateScript(GetProfileResponse profile)
        {
            if (profile == null) return _rawInitScript;

            var visitedAlltimeReplacement = GetVisitedAlltimeReplacement(profile, false);
            var visitedYear = profile.PlacesInYearScore.Select(p => p.ScriptId).ToList();

            var script = _rawInitScript;

            if (!string.IsNullOrEmpty(visitedAlltimeReplacement))
            {
                script = script.Replace("\"GetVisitedAlltime\"", visitedAlltimeReplacement);
            }

            if (visitedYear.Count > 0)
            {
                script = script.Replace("\"GetVisitedYear\"", string.Join(",", visitedYear));
            }

            return script;
        }

        public string GetVisitedAlltimeReplacement(GetProfileResponse profile, bool includeYear)
        {
            var visitedAlltime = profile.PlacesInAllTimeScore
                .Where(p => includeYear || !profile.PlacesInYearScore.Any(y => y.Id == p.Id))
                .Select(p => p.ScriptId)
                .OrderBy(id => id)
                .ToList();

            return visitedAlltime.Count > 0 ? string.Join(",", visitedAlltime) : string.Empty;
        }

        private async void OnToggleVisibilityChanged(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var value = _chkToggleVisibility.IsChecked == true ? "true" : "false";
            var script = $"window.postMessage( {{ command:'toggleLayer', visible: {value} }});";

            try
            {
                await _webView.InvokeScript(script);
            }
            catch
            {
                // ignore
            }

            var oldUncheckedState = _btnUnCheckRoute.IsEnabled;

            _btnCheckRoute.IsEnabled = _chkToggleVisibility.IsChecked == true;
            _btnUnCheckRoute.IsEnabled = _oldUncheckedState;

            _oldUncheckedState = oldUncheckedState;
        }

        public async void ClickHandlerCheckRoute(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_multiSportManager.RunWithSource == "ridewithgps")
            {
                await DoCheckRouteRwGps();
            }
            else
            {
                await DoCheckRouteStrava();
            }
        }

        private async Task DoCheckRouteRwGps()
        {
            var script = ScriptProvider.GetRwGpsTrackRetrievalScript();

            string coordinates;
            try
            {
                coordinates = await _webView.InvokeScript(script);
            }
            catch
            {
                return;
            }

            if (!string.IsNullOrEmpty(coordinates) && coordinates != "[]")
            {
                try
                {
                    var trackPoints = JsonConvert.DeserializeObject<List<double[]>>(coordinates);
                    var request = new CheckLineStringsRequest { Lines = new List<List<double[]>> { trackPoints } };

                    var token = await _webViewConnector.Login();
                    var places = await _ltc2Proxy.CheckLineStrings(token, request);

                    _btnUnCheckRoute.IsEnabled = places.Count > 0;

                    var updateScript = ScriptProvider.GetRwGpsTrackUpdateScript(places);

                    var visitedAlltimeReplacement = GetVisitedAlltimeReplacement(_profile, true);
                    if (!string.IsNullOrEmpty(visitedAlltimeReplacement))
                    {
                        updateScript = updateScript.Replace("\"GetVisitedAlltime\"", visitedAlltimeReplacement);
                    }

                    await _webView.InvokeScript(updateScript);
                }
                catch
                {
                    // ignore
                }
            }
            else
            {
                _btnUnCheckRoute.IsEnabled = false;

                try
                {
                    await _webView.InvokeScript(ScriptProvider.GetRwGpsTrackUpdateScript(new List<string>()));
                }
                catch
                {
                    // ignore
                }
            }
        }

        private async Task DoCheckRouteStrava()
        {
            var script = ScriptProvider.GetStravaTrackRetrievalScript();

            string coordinates;
            try
            {
                coordinates = await _webView.InvokeScript(script);
            }
            catch
            {
                return;
            }

            if (!string.IsNullOrEmpty(coordinates) && coordinates != "[]")
            {
                try
                {
                    var trackPoints = JsonConvert.DeserializeObject<List<List<double[]>>>(coordinates);
                    var request = new CheckLineStringsRequest { Lines = trackPoints };

                    var token = await _webViewConnector.Login();
                    var places = await _ltc2Proxy.CheckLineStrings(token, request);

                    _btnUnCheckRoute.IsEnabled = places.Count > 0;

                    var updateScript = ScriptProvider.GetStravaTrackUpdateScript(places);

                    var visitedAlltimeReplacement = GetVisitedAlltimeReplacement(_profile, true);
                    if (!string.IsNullOrEmpty(visitedAlltimeReplacement))
                    {
                        updateScript = updateScript.Replace("\"GetVisitedAlltime\"", visitedAlltimeReplacement);
                    }

                    await _webView.InvokeScript(updateScript);
                    await _webView.InvokeScript(ScriptProvider.GetStravaDrawRouteScript());
                }
                catch
                {
                    // ignore
                }
            }
            else
            {
                _btnUnCheckRoute.IsEnabled = false;

                try
                {
                    await _webView.InvokeScript(ScriptProvider.GetStravaTrackUpdateScript(new List<string>()));
                    await _webView.InvokeScript(ScriptProvider.GetStravaClearRouteScript());
                }
                catch
                {
                    // ignore
                }
            }
        }

        public async void ClickHandlerUnCheckRoute(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _btnUnCheckRoute.IsEnabled = false;

            var updateScript = _multiSportManager.RunWithSource == "ridewithgps"
                ? ScriptProvider.GetRwGpsTrackUpdateScript(new List<string>())
                : ScriptProvider.GetStravaTrackUpdateScript(new List<string>());

            try
            {
                await _webView.InvokeScript(updateScript);

                if (_multiSportManager.RunWithSource != "ridewithgps")
                {
                    await _webView.InvokeScript(ScriptProvider.GetStravaClearRouteScript());
                }
            }
            catch
            {
                // ignore
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (WindowState != WindowState.Minimized)
            {
                _previousWindowState = WindowState;
            }

            base.OnClosed(e);
        }

        private void OnClosing(object sender, WindowClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void EnableDisableSpinner(bool enable)
        {
            if (_progressRing != null)
            {
                _progressRing.IsActive = enable;
                _progressRing.IsVisible = enable;
            }
        }

        private string GetStartUrl()
        {
            return _multiSportManager.RunWithSource == "ridewithgps"
                ? _appSettings.RideWithGpsRouteBuilderEntryPoint
                : _appSettings.StravaRouteBuilderEntryPoint;
        }

        private bool IsBuilderUrl(string completeUrl)
        {
            if (string.IsNullOrEmpty(completeUrl)) return false;

            var url = completeUrl.Split('?')[0];

            if (_multiSportManager.RunWithSource == "ridewithgps")
            {
                return _appSettings.RideWithGpsRouteBuilderRegex != null
                    && Regex.IsMatch(url, _appSettings.RideWithGpsRouteBuilderRegex);
            }
            else
            {
                return _appSettings.StravaRouteBuilderRegex != null
                    && Regex.IsMatch(url, _appSettings.StravaRouteBuilderRegex);
            }
        }

        private string GetSitePrefix()
        {
            return _multiSportManager.RunWithSource == "ridewithgps"
                ? _appSettings.RideWithGpsSitePrefix
                : _appSettings.StravaSitePrefix;
        }

        private string GetInitScript()
        {
            return _multiSportManager.RunWithSource == "ridewithgps"
                ? ScriptProvider.GetRwGpsRouteBuilderScript()
                : ScriptProvider.GetStravaRouteBuilderScript();
        }
    }
}
