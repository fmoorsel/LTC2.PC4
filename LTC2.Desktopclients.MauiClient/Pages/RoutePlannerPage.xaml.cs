using LTC2.Desktopclients.MauiClient.Models;
using LTC2.Desktopclients.MauiClient.Services;
using LTC2.Desktopclients.MauiClient.Utils;
using LTC2.Shared.BaseMessages.Interfaces;
using LTC2.Shared.Http.Interfaces;
using LTC2.Shared.Models.Requests;
using LTC2.Shared.Models.Responses;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LTC2.Desktopclients.MauiClient.Pages
{
    public partial class RoutePlannerPage : ContentPage
    {
        private bool _isInitialized;
        private bool _oldUncheckedState;

        private string _rawInitScript;
        private string _initScript;

        private readonly object _profileLock = new object();
        private GetProfileResponse _profile;

        private readonly AppSettings _appSettings;
        private readonly MultiSportManager _multiSportManager;
        private readonly IBaseTranslationService _translationService;
        private readonly ILTC2HttpProxy _ltc2Proxy;
        private readonly WebViewConnector _webViewConnector;

        public RoutePlannerPage(
            AppSettings appSettings,
            MultiSportManager multiSportManager,
            IBaseTranslationService translationService,
            WebViewConnector webViewConnector,
            ILTC2HttpProxy ltc2Proxy)
        {
            _appSettings = appSettings;
            _multiSportManager = multiSportManager;
            _translationService = translationService;
            _webViewConnector = webViewConnector;
            _ltc2Proxy = ltc2Proxy;

            InitializeComponent();

            Title = _translationService.GetMessage("title.routeplanner.window");
            BtnCheckRoute.Text = _translationService.GetMessage("button.checkroute");
            BtnUnCheckRoute.Text = _translationService.GetMessage("button.uncheckroute");
            LblToggleVisibility.Text = _translationService.GetMessage("check.toggle.visibility");

            WebView.NavigationStarting += (s, e) => MainThread.InvokeOnMainThreadAsync(() => EnableDisableSpinner(true));
            WebView.NavigationCompleted += (s, url) => OnNavigationCompleted(s, url);
            WebView.WebMessageReceived += OnWebMessageReceived;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!_isInitialized)
            {
                _isInitialized = true;
                _rawInitScript = GetInitScript();

                try
                {
                    if (!string.IsNullOrEmpty(_appSettings.WebviewRoot) && !Directory.Exists(_appSettings.WebviewRoot))
                        Directory.CreateDirectory(_appSettings.WebviewRoot);

                    WebView.WebviewRoot = _appSettings.WebviewRoot;
                    await WebView.EnsureInitializedAsync();
                    await WebView.NavigateAsync(GetStartUrl());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"RoutePlannerPage init error: {ex.Message}");
                }
            }
        }

        private void EnableDisableSpinner(bool enable)
        {
            CtrProgressRing.IsRunning = enable;
            CtrProgressRing.IsVisible = enable;
        }

        private async void OnNavigationCompleted(object sender, string url)
        {
            MainThread.InvokeOnMainThreadAsync(() => EnableDisableSpinner(false));

            if (string.IsNullOrEmpty(url))
                url = await WebView.EvaluateJavaScriptAsync("window.location.href") ?? string.Empty;
            url = url?.Trim('"') ?? string.Empty;

            if (!string.IsNullOrEmpty(url) && url.StartsWith(GetStartUrl()) && GetProfile() == null)
            {
                UpdateProfile();
            }

            if (IsBuilderUrl(url))
            {
                _ = InitBuilderAsync();
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    ChkToggleVisibility.IsVisible = true;
                    LblToggleVisibility.IsVisible = true;
                    BtnCheckRoute.IsVisible = true;
                    BtnUnCheckRoute.IsVisible = true;
                    BtnUnCheckRoute.IsEnabled = false;
                    _oldUncheckedState = false;
                });
            }
            else
            {
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    LblCurrentPlace.Text = string.Empty;
                    ChkToggleVisibility.IsVisible = false;
                    LblToggleVisibility.IsVisible = false;
                    BtnCheckRoute.IsVisible = false;
                    BtnUnCheckRoute.IsVisible = false;
                    BtnUnCheckRoute.IsEnabled = false;
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
                try { result = await WebView.EvaluateJavaScriptAsync(_initScript); }
                catch { result = null; }

                if (result?.Trim('"') == "1") return;

                await Task.Delay(200);
                profile = await EnsureProfile();
                if (profile == null) return;
            }

            if (result?.Trim('"') != "1")
            {
                var msg = _translationService.GetMessage("plugin.install.failed");
                var header = _translationService.GetMessage("plugin.install.failed.header");
                MainThread.InvokeOnMainThreadAsync(async () => await DisplayAlert(header, msg, "OK"));
            }
        }

        private void OnWebMessageReceived(object sender, string parameter)
        {
            if (parameter.StartsWith("not supported"))
            {
                MainThread.InvokeOnMainThreadAsync(() =>
                    LblCurrentPlace.Text = _translationService.GetMessage("routeplanner.no_map_support"));
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
                    labelText = _translationService.GetMessage("routeplanner.postcode.year", postcode);
                else if (visitedAlltime != null)
                    labelText = _translationService.GetMessage("routeplanner.postcode.visited", postcode);
                else
                    labelText = _translationService.GetMessage("routeplanner.postcode", postcode);
            }
            else
            {
                labelText = _translationService.GetMessage("routeplanner.postcode", postcode);
            }

            MainThread.InvokeOnMainThreadAsync(() => LblCurrentPlace.Text = labelText);
        }

        private GetProfileResponse GetProfile()
        {
            lock (_profileLock) { return _profile; }
        }

        private async Task<GetProfileResponse> EnsureProfile()
        {
            if (GetProfile() == null) SetProfile(await RetrieveProfile());
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
                script = script.Replace("\"GetVisitedAlltime\"", visitedAlltimeReplacement);
            if (visitedYear.Count > 0)
                script = script.Replace("\"GetVisitedYear\"", string.Join(",", visitedYear));

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

        private async void OnToggleVisibility(object sender, CheckedChangedEventArgs e)
        {
            var value = ChkToggleVisibility.IsChecked ? "true" : "false";
            var script = $"window.postMessage( {{ command:'toggleLayer', visible: {value} }});";
            try { await WebView.EvaluateJavaScriptAsync(script); }
            catch { }

            var oldUncheckedState = BtnUnCheckRoute.IsEnabled;
            BtnCheckRoute.IsEnabled = ChkToggleVisibility.IsChecked;
            BtnUnCheckRoute.IsEnabled = _oldUncheckedState;
            _oldUncheckedState = oldUncheckedState;
        }

        private async void OnCheckRoute(object sender, EventArgs e)
        {
            if (_multiSportManager.RunWithSource == "ridewithgps")
                await DoCheckRouteRwGps();
            else
                await DoCheckRouteStrava();
        }

        private async Task DoCheckRouteRwGps()
        {
            var script = ScriptProvider.GetRwGpsTrackRetrievalScript();
            string coordinates;
            try { coordinates = await WebView.EvaluateJavaScriptAsync(script); }
            catch { return; }

            if (!string.IsNullOrEmpty(coordinates) && coordinates != "[]")
            {
                try
                {
                    var trackPoints = JsonConvert.DeserializeObject<List<double[]>>(coordinates);
                    var request = new CheckLineStringsRequest { Lines = new List<List<double[]>> { trackPoints } };
                    var token = await _webViewConnector.Login();
                    var places = await _ltc2Proxy.CheckLineStrings(token, request);
                    BtnUnCheckRoute.IsEnabled = places.Count > 0;

                    var updateScript = ScriptProvider.GetRwGpsTrackUpdateScript(places);
                    var visitedAlltimeReplacement = GetVisitedAlltimeReplacement(_profile, true);
                    if (!string.IsNullOrEmpty(visitedAlltimeReplacement))
                        updateScript = updateScript.Replace("\"GetVisitedAlltime\"", visitedAlltimeReplacement);

                    await WebView.EvaluateJavaScriptAsync(updateScript);
                }
                catch { }
            }
            else
            {
                BtnUnCheckRoute.IsEnabled = false;
                try { await WebView.EvaluateJavaScriptAsync(ScriptProvider.GetRwGpsTrackUpdateScript(new List<string>())); }
                catch { }
            }
        }

        private async Task DoCheckRouteStrava()
        {
            var script = ScriptProvider.GetStravaTrackRetrievalScript();
            string coordinates;
            try { coordinates = await WebView.EvaluateJavaScriptAsync(script); }
            catch { return; }

            if (!string.IsNullOrEmpty(coordinates) && coordinates != "[]")
            {
                try
                {
                    var trackPoints = JsonConvert.DeserializeObject<List<List<double[]>>>(coordinates);
                    var request = new CheckLineStringsRequest { Lines = trackPoints };
                    var token = await _webViewConnector.Login();
                    var places = await _ltc2Proxy.CheckLineStrings(token, request);
                    BtnUnCheckRoute.IsEnabled = places.Count > 0;

                    var updateScript = ScriptProvider.GetStravaTrackUpdateScript(places);
                    var visitedAlltimeReplacement = GetVisitedAlltimeReplacement(_profile, true);
                    if (!string.IsNullOrEmpty(visitedAlltimeReplacement))
                        updateScript = updateScript.Replace("\"GetVisitedAlltime\"", visitedAlltimeReplacement);

                    await WebView.EvaluateJavaScriptAsync(updateScript);
                    await WebView.EvaluateJavaScriptAsync(ScriptProvider.GetStravaDrawRouteScript());
                }
                catch { }
            }
            else
            {
                BtnUnCheckRoute.IsEnabled = false;
                try
                {
                    await WebView.EvaluateJavaScriptAsync(ScriptProvider.GetStravaTrackUpdateScript(new List<string>()));
                    await WebView.EvaluateJavaScriptAsync(ScriptProvider.GetStravaClearRouteScript());
                }
                catch { }
            }
        }

        private async void OnUnCheckRoute(object sender, EventArgs e)
        {
            BtnUnCheckRoute.IsEnabled = false;
            var updateScript = _multiSportManager.RunWithSource == "ridewithgps"
                ? ScriptProvider.GetRwGpsTrackUpdateScript(new List<string>())
                : ScriptProvider.GetStravaTrackUpdateScript(new List<string>());

            try
            {
                await WebView.EvaluateJavaScriptAsync(updateScript);
                if (_multiSportManager.RunWithSource != "ridewithgps")
                    await WebView.EvaluateJavaScriptAsync(ScriptProvider.GetStravaClearRouteScript());
            }
            catch { }
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
                return _appSettings.RideWithGpsRouteBuilderRegex != null && Regex.IsMatch(url, _appSettings.RideWithGpsRouteBuilderRegex);
            else
                return _appSettings.StravaRouteBuilderRegex != null && Regex.IsMatch(url, _appSettings.StravaRouteBuilderRegex);
        }

        private string GetInitScript()
        {
            return _multiSportManager.RunWithSource == "ridewithgps"
                ? ScriptProvider.GetRwGpsRouteBuilderScript()
                : ScriptProvider.GetStravaRouteBuilderScript();
        }
    }
}
