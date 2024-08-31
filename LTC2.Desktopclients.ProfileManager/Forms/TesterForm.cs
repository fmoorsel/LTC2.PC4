using LTC2.Desktopclients.ProfileManager.Models;
using LTC2.Shared.Messages.Interfaces;
using Microsoft.Web.WebView2.Core;

namespace LTC2.Desktopclients.ProfileManager.Forms
{
    public partial class TesterForm : Form
    {
        public bool IsTestSuccessFull { get; set; }

        public string ProfileToTest { get; set; }

        public string AthleteId { get; set; }


        private Form _callingForm;

        private readonly AppSettings _appSettings;
        private readonly ITranslationService _translationService;

        private bool _isWebFormInitialized;

        public TesterForm(AppSettings appSettings, ITranslationService translationService)
        {
            InitializeComponent();

            _appSettings = appSettings;
            _translationService = translationService;

            _translationService.LoadMessagesForForm(this);
        }

        private async void TesterForm_VisibleChanged(object sender, EventArgs e)
        {
            _callingForm = sender as Form;

            if (Visible)
            {
                IsTestSuccessFull = false;
                AthleteId = null;
                btnLinkProfile.Enabled = false;
            }

            if (!_isWebFormInitialized)
            {
                await InitWebview();

                _isWebFormInitialized = true;
            }

            await DeleteStravaCookies();

            webView.CoreWebView2.Navigate($"{_appSettings.StartPage}?profile={ProfileToTest}&language={_translationService.CurrentLanguage}");
        }

        private async Task DeleteStravaCookies()
        {
            var stravaCookies = await webView.CoreWebView2.CookieManager.GetCookiesAsync("https://www.strava.com");

            foreach (var cookie in stravaCookies)
            {
                webView.CoreWebView2.CookieManager.DeleteCookie(cookie);
            }
        }

        private void TesterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;

            Hide();
        }

        private void webView_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            pbxBrowsing.Visible = true;
        }

        private void webView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            pbxBrowsing.Visible = false;
        }

        private void pnlBar_Resize(object sender, EventArgs e)
        {
            pnlButtons.Left = (int)(pnlBar.Width * 0.5f - pnlButtons.Width * 0.5f);
            btnLinkProfile.Left = (int)(pnlButtons.Width * 0.5f - btnLinkProfile.Width * 0.5f);
        }

        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;
        }

        private void TesterForm_Load(object sender, EventArgs e)
        {
            pnlBar_Resize(sender, e);
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
            webView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
        }

        private async void tmrCheckTest_Tick(object sender, EventArgs e)
        {
            var athleteId = await GetAthleteId();

            btnLinkProfile.Enabled = athleteId != null;
        }

        private async void btnLinkProfile_Click(object sender, EventArgs e)
        {
            var athleteId = await GetAthleteId();

            IsTestSuccessFull = athleteId != null;
            AthleteId = athleteId;

            ProfileToTest = athleteId;

            Hide();
        }

        private async Task<string> GetAthleteId()
        {
            try
            {
                var result = await webView.ExecuteScriptAsync("StravaAthleteId()");
                var success = result != "null" && result != null;

                if (success)
                {
                    return result.Trim('"');
                }
            }
            catch (Exception)
            {
            }

            return null;
        }
    }
}
