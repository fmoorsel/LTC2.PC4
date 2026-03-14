using LTC2.Desktopclients.WindowsClient.Models;
using LTC2.Desktopclients.WindowsClient.Services;
using LTC2.Shared.Http.Interfaces;
using LTC2.Shared.Messages.Interfaces;
using LTC2.Shared.Models.Responses;
using Microsoft.Web.WebView2.Core;
using System.Diagnostics;

namespace LTC2.Desktopclients.WindowsClient.Forms
{
    public partial class RoutePlanner : Form
    {
        private readonly AppSettings _appSettings;
        private readonly MultiSportManager _multiSportsManager;
        private readonly ITranslationService _translationService;
        private readonly ILTC2HttpProxy _ltc2Proxy;
        private readonly WebviewConnector _webviewConnector;

        private FormWindowState _previousWindowState;

        private string _rawInitScript;
        private string _initScript;

        private readonly object _profileLock = new object();
        private GetProfileResponse _profile;

        public RoutePlanner(
            AppSettings appSettings,
            ITranslationService translationService,
            WebviewConnector webviewConnector,
            ILTC2HttpProxy ltc2Proxy,
            MultiSportManager multiSportsManager)
        {
            InitializeComponent();

            this.AutoScaleMode = AutoScaleMode.Dpi;

            _appSettings = appSettings;
            _multiSportsManager = multiSportsManager;
            _translationService = translationService;
            _ltc2Proxy = ltc2Proxy;
            _webviewConnector = webviewConnector;

            _previousWindowState = FormWindowState.Normal;
        }

        public async Task InitRoutePlanner()
        {
            if (!Directory.Exists(_appSettings.WebviewRoot))
            {
                Directory.CreateDirectory(_appSettings.WebviewRoot);
            }

            _rawInitScript = GetInitScript();

            var env = await CoreWebView2Environment.CreateAsync(null, _appSettings.WebviewRoot, null);

            await webView.EnsureCoreWebView2Async(env);

            webView.Source = new Uri("about:blank", UriKind.Absolute);

            webView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
        }

        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;
        }

        private void webView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            pbxBrowsing.Visible = true;
        }

        private void webView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            pbxBrowsing.Visible = false;

            if (string.IsNullOrEmpty(_rawInitScript))
            {
                return;
            }

            if (webView.CoreWebView2.Source.StartsWith(GetStartUrl()) && GetProfile() == null)
            {
                UpdateProfile();
            }

            if (webView.CoreWebView2.Source.StartsWith(GetBuilderPrefix()))
            {
                Task.Run(async () =>
                {
                    var profile = await EnsureProfile();

                    var r = profile == null ? null : await webView.ExecuteScriptAsync(_initScript);
                    var attempts = 1;

                    while (r == null || r.Trim('"') != "1" || attempts > 20)
                    {
                        await Task.Delay(200);

                        profile = await EnsureProfile();

                        r = profile == null ? null : await webView.ExecuteScriptAsync(_initScript);
                    }

                    if (r.Trim('"') != "1")
                    {
                        var msg = _translationService.GetMessage("#plugin.install.failed");
                        var header = _translationService.GetMessage("#plugin.install.failed.header");

                        MessageBox.Show(msg, header);
                    }
                });
            }
            else
            {
                lblCurrentPlace.Text = string.Empty;
            }
        }

        private string CreateScript(GetProfileResponse profile)
        {
            if (profile != null)
            {
                var visitedAlltime = profile.PlacesInAllTimeScore
                            .Where(p => !profile.PlacesInYearScore.Any(y => y.Id == p.Id))
                            .Select(p => p.ScriptId).ToList();

                var visitedYear = profile.PlacesInYearScore
                            .Select(p => p.ScriptId).ToList();

                var visitedAlltimeString = string.Join(",", visitedAlltime);
                var visitedYearString = string.Join(",", visitedYear);

                var script = _rawInitScript.Replace("\"GetVisitedAlltime\"", visitedAlltimeString);
                script = script.Replace("\"GetVisitedYear\"", visitedYearString);

                return script;
            }

            return _rawInitScript;
        }

        private void RoutePlanner_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();

            e.Cancel = true;
        }

        private void RoutePlanner_Resize(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
            {
                _previousWindowState = WindowState;
            }
        }

        public void ShowPlanner()
        {
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = _previousWindowState;
            }

            pnlBar_Resize(this, EventArgs.Empty);

            Show();
            Activate();

            if (webView.CoreWebView2.Source == null || !webView.CoreWebView2.Source.ToString().StartsWith(GetSitePrefix()))
            {
                lblCurrentPlace.Text = string.Empty;

                webView.CoreWebView2.Navigate(GetStartUrl());
            }
            else if (webView.CoreWebView2.Source.ToString().StartsWith(GetBuilderPrefix()))
            {
                lblCurrentPlace.Text = "Postcode: ----";
            }
            else
            {
                lblCurrentPlace.Text = string.Empty;
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
            var token = await _webviewConnector.Login();
            var profile = await _ltc2Proxy.GetProfile(token);

            return profile;
        }

        public void UpdateProfile()
        {
            Task.Run(async () =>
            {
                var profile = await RetrieveProfile();

                lock (_profileLock)
                {
                    _profile = profile;

                    _initScript = CreateScript(profile);
                }
            });
        }

        private string GetStartUrl()
        {
            if (_multiSportsManager.RunWithSource == "ridewithgps")
            {
                return _appSettings.RideWithGpsRouteBuilderEntryPoint;
            }
            else
            {
                return _appSettings.StravaRouteBuilderEntryPoint;
            }
        }

        private string GetBuilderPrefix()
        {
            if (_multiSportsManager.RunWithSource == "ridewithgps")
            {
                return _appSettings.RideWithGpsRouteBuilderPrefix;
            }
            else
            {
                return _appSettings.StravaRouteBuilderPrefix;
            }
        }

        private string GetSitePrefix()
        {
            if (_multiSportsManager.RunWithSource == "ridewithgps")
            {
                return _appSettings.RideWithGpsSitePrefix;
            }
            else
            {
                return _appSettings.StravaSitePrefix;
            }
        }

        private string GetInitScript()
        {
            var fileName = _multiSportsManager.RunWithSource == "ridewithgps" ? "rwgps.init" : "strava.init";
            var processModule = Process.GetCurrentProcess().MainModule;
            var folder = Path.Combine(Path.GetDirectoryName(processModule?.FileName), "Resources");

            var activitiesFile = Path.Combine(folder, fileName);
            var content = File.ReadAllText(activitiesFile);

            return content;
        }

        private void webView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var parameter = e.TryGetWebMessageAsString();
            var parts = parameter.Split(',');

            var postcode = parts[0];
            var id = parts[1].Split(':')[0];

            var profile = GetProfile();

            if (profile != null)
            {
                var visitedAlltime = profile.PlacesInAllTimeScore.FirstOrDefault(p => p.Id == id);
                var visitedYear = profile.PlacesInYearScore.FirstOrDefault(p => p.Id == id);

                if (visitedAlltime != null && visitedYear != null)
                {
                    lblCurrentPlace.Text = _translationService.GetMessage("#routeplanner.postcode.year", postcode);
                }
                else if (visitedAlltime != null)
                {
                    lblCurrentPlace.Text = _translationService.GetMessage("#routeplanner.postcode.visited", postcode);
                }
                else
                {
                    lblCurrentPlace.Text = _translationService.GetMessage("#routeplanner.postcode", postcode);
                }
            }
            else
            {
                lblCurrentPlace.Text = _translationService.GetMessage("#routeplanner.postcode", postcode);
            }

            lblCurrentPlace.Left = (int)(pnlPlace.Width * 0.5f - lblCurrentPlace.Width * 0.5f);
        }

        private void pnlBar_Resize(object sender, EventArgs e)
        {
            pnlPlace.Left = (int)(pnlBar.Width * 0.5f - pnlPlace.Width * 0.5f);
            lblCurrentPlace.Left = (int)(pnlPlace.Width * 0.5f - lblCurrentPlace.Width * 0.5f);
        }

        private void RoutePlanner_Load(object sender, EventArgs e)
        {
            pnlBar_Resize(sender, e);
        }
    }
}
