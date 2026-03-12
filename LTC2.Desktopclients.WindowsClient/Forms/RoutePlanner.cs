using LTC2.Desktopclients.WindowsClient.Models;
using LTC2.Desktopclients.WindowsClient.Services;
using LTC2.Shared.Messages.Interfaces;
using Microsoft.Web.WebView2.Core;
using System.Diagnostics;

namespace LTC2.Desktopclients.WindowsClient.Forms
{
    public partial class RoutePlanner : Form
    {
        private readonly AppSettings _appSettings;
        private readonly MultiSportManager _multiSportsManager;
        private readonly ITranslationService _translationService;

        private FormWindowState _previousWindowState;

        private string _initScript;

        public RoutePlanner(
            AppSettings appSettings,
            ITranslationService translationService,
            MultiSportManager multiSportsManager)
        {
            InitializeComponent();

            this.AutoScaleMode = AutoScaleMode.Dpi;

            _appSettings = appSettings;
            _multiSportsManager = multiSportsManager;
            _translationService = translationService;

            _previousWindowState = FormWindowState.Normal;
        }

        public async Task InitRoutePlanner()
        {
            if (!Directory.Exists(_appSettings.WebviewRoot))
            {
                Directory.CreateDirectory(_appSettings.WebviewRoot);
            }

            _initScript = GetInitScript();

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

            if (string.IsNullOrEmpty(_initScript))
            {
                return;
            }

            if (webView.CoreWebView2.Source.StartsWith(GetBuilderPrefix()))
            {
                Task.Run(async () =>
                {
                    var r = await webView.ExecuteScriptAsync(_initScript);
                    var attempts = 1;

                    while (r == null || r.Trim('"') != "1" || attempts > 20)
                    {
                        await Task.Delay(200);

                        r = await webView.ExecuteScriptAsync(_initScript);
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
            var fileName = _multiSportsManager.RunWithSource == "ridewithgps" ? "RideWithGps.js" : "strava.init";
            var processModule = Process.GetCurrentProcess().MainModule;
            var folder = Path.Combine(Path.GetDirectoryName(processModule?.FileName), "Resources");

            var activitiesFile = Path.Combine(folder, fileName);
            var content = File.ReadAllText(activitiesFile);

            return content;
        }

        private void webView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            lblCurrentPlace.Text = $"Postcode: {e.TryGetWebMessageAsString()}";
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
