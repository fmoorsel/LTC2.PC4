using LTC2.Desktopclients.WindowsClient.Models;
using LTC2.Desktopclients.WindowsClient.Services;
using LTC2.Shared.Messages.Interfaces;
using LTC2.Shared.Models.Interprocess;
using Microsoft.Web.WebView2.Core;

namespace LTC2.Desktopclients.WindowsClient.Forms
{
    public partial class MainForm : Form
    {
        private readonly SplashScreen _splashScreen;
        private readonly StatusNotifier _statusNotifier;
        private readonly UpdateActivitiesForm _updateActivities;
        private readonly WebviewConnector _webviewConnector;
        private readonly AppSettings _appSettings;
        private readonly ProfileManager _profileManager;
        private readonly ITranslationService _translationService;

        private bool _inFatalMode;
        private bool _isUpdating;

        private List<string> _refreshEnabledFor;

        public MainForm(
            SplashScreen splashScreen,
            StatusNotifier statusNotifier,
            UpdateActivitiesForm updateActivities,
            WebviewConnector webviewConnector,
            ProfileManager profileManager,
            ITranslationService translationService,
            AppSettings appSettings)
        {
            InitializeComponent();

            this.AutoScaleMode = AutoScaleMode.Dpi;

            _splashScreen = splashScreen;
            _statusNotifier = statusNotifier;
            _updateActivities = updateActivities;
            _webviewConnector = webviewConnector;
            _profileManager = profileManager;
            _appSettings = appSettings;
            _translationService = translationService;

            _inFatalMode = false;

            _refreshEnabledFor = appSettings?.EnableRefreshFor.Split(',').ToList();

            _translationService.LoadMessagesForForm(this);
        }

        public async Task LoadAppInWebView()
        {
            Show();

            await InitWebview();

            _statusNotifier.OnStatusNotification += OnStatusNotification;

            tmrKeepAlive.Enabled = true;

            webView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;

            webView.CoreWebView2.Navigate(GetUrl());

            _webviewConnector.WebView = webView;
        }

        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;
        }

        private delegate void UpdateStatusDelegate(StatusMessage status);

        private void OnStatusNotification(object sender, OnStatusMessageEventArguments e)
        {
            if (InvokeRequired)
            {
                var updater = new UpdateStatusDelegate(UpdateStatus);

                BeginInvoke(updater, e.Status);
            }
            else
            {
                UpdateStatus(e.Status);
            }
        }

        private void UpdateStatus(StatusMessage status)
        {
            var component = GetComponentNiceName(status.Origin);

            if (status.Status == StatusMessage.STATUS_PING)
            {
                var msg = GetDateFromPing(status.Message);
                var labelText = $"{component} {msg}";

                if (status.Origin == StatusMessage.ORG_CALCULATOR)
                {
                    lblCalculatorStatus.Text = labelText;
                }
                else if (status.Origin == StatusMessage.ORG_WEBAPP)
                {
                    lblWebappStatus.Text = labelText;
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

                lblUpdateProgress.Text = $" {msg}";
            }
            else if (status.Status == StatusMessage.STATUS_RESULT)
            {
                lblUpdateProgress.Text = "";

                webView.CoreWebView2.Navigate(GetUrl());
            }
            else if (status.Status == StatusMessage.STATUS_LIMIT)
            {
                lblUpdateProgress.Text = _translationService.GetMessage("progress.limit");

                webView.CoreWebView2.Navigate(GetUrl());
            }
            else if (status.Status == StatusMessage.STATUS_WAIT)
            {
                lblUpdateProgress.Text = $" {_translationService.GetMessage("progress.quater.limit")} {status.Message}";
            }
            else if (status.Status == StatusMessage.STATUS_FATAL && !_inFatalMode)
            {
                _inFatalMode = true;

                var text = _translationService.GetMessage("messagebox.fatal.component.exit", component);
                var caption = _translationService.GetMessage("messagebox.fatal.component.exit.header");

                MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);

                Close();
            }
        }

        private string GetDateFromPing(string msg)
        {
            if (msg == null)
            {
                return string.Empty;
            }
            else
            {
                try
                {
                    return DateTime.SpecifyKind(DateTime.Parse(msg), DateTimeKind.Utc).ToLocalTime().ToString();
                }
                catch (Exception)
                {
                    return string.Empty;
                }
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
            else
            {
                return string.Empty;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            pnlBar_Resize(sender, e);

            _splashScreen.OpenFromMainForm(this);
        }

        private async Task InitWebview()
        {
            if (!Directory.Exists(_appSettings.WebviewRoot))
            {
                Directory.CreateDirectory(_appSettings.WebviewRoot);
            }

            var env = await CoreWebView2Environment.CreateAsync(null, _appSettings.WebviewRoot, null);

            await webView.EnsureCoreWebView2Async(env);

            webView.Source = new Uri("about:blank", UriKind.Absolute);
            webView.CoreWebView2.Settings.IsPasswordAutosaveEnabled = !_appSettings.DisablePasswordSave;

            if (_profileManager.HasMultipleProfiles)
            {
                await DeleteStravaCookies();
            }
        }

        private async Task DeleteStravaCookies()
        {
            var stravaCookies = await webView.CoreWebView2.CookieManager.GetCookiesAsync("https://www.strava.com");

            foreach (var cookie in stravaCookies)
            {
                webView.CoreWebView2.CookieManager.DeleteCookie(cookie);
            }
        }

        private void tmrKeepAlive_Tick(object sender, EventArgs e)
        {
            if (!_inFatalMode)
            {
                _statusNotifier.CheckKeepAliveStatuses();
            }
        }

        private void webView_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            pbxBrowsing.Visible = true;
            btnRefresh.Enabled = false;
        }

        private void webView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            pbxBrowsing.Visible = false;

            if (_refreshEnabledFor != null)
            {
                btnRefresh.Enabled = _refreshEnabledFor.Contains(webView.CoreWebView2.Source);

                if (btnRefresh.Enabled)
                {
                    btnRefresh.BackgroundImage = Properties.Resources.refresh2;
                }
                else
                {
                    btnRefresh.BackgroundImage = Properties.Resources.refresh3;
                }
            }
        }

        private void pnlBar_Resize(object sender, EventArgs e)
        {
            pnlButtons.Left = (int)(pnlBar.Width * 0.5f - pnlButtons.Width * 0.5f);
            btnRefresh.Left = (int)(pnlButtons.Width * 0.5f - btnRefresh.Width * 0.5f);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            _updateActivities.ShowDialog();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isUpdating)
            {
                var answer = MessageBox.Show(_translationService.GetMessage("messagebox.exit.confirm"), _translationService.GetMessage("messagebox.exit.confirm.header"), MessageBoxButtons.YesNo);

                e.Cancel = (answer == DialogResult.No);
            }
        }

        private string GetUrl()
        {
            return $"{_appSettings.StartPage}?language={_translationService.CurrentLanguage}";
        }
    }
}
