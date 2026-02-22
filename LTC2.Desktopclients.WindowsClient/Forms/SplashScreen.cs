using LTC2.Desktopclients.WindowsClient.Forms;
using LTC2.Desktopclients.WindowsClient.Models;
using LTC2.Desktopclients.WindowsClient.Services;
using LTC2.Shared.Messages.Interfaces;
using LTC2.Shared.Models.Interprocess;
using System.Diagnostics;

namespace LTC2.Desktopclients.WindowsClient
{
    public partial class SplashScreen : Form
    {
        private readonly StatusNotifier _statusNotifier;
        private readonly ITranslationService _translationService;
        private readonly MultiSportManager _multiSportManager;
        private readonly ProfileManager _profileManager;

        private bool _started;
        private bool _startViewer;
        private bool _calculatorStarted;
        private bool _webappStarted;
        //        private bool _profileDependenciesSet;

        private MainForm _mainForm;

        private readonly AppSettings _appSettings;

        public SplashScreen(
            StatusNotifier statusNotifier,
            ITranslationService translationService,
            MultiSportManager multiSportManager,
            ProfileManager profileManager,
            AppSettings appSettings)
        {
            InitializeComponent();

            _statusNotifier = statusNotifier;

            _statusNotifier.OnStatusNotification += OnStatusNotification;
            _translationService = translationService;
            _multiSportManager = multiSportManager;
            _profileManager = profileManager;

            _translationService.LoadMessagesForForm(this);

            _appSettings = appSettings;
        }

        public void OpenFromMainForm(MainForm mainForm)
        {
            _mainForm = mainForm;

            ShowDialog();
        }


        private void SetProfileDependencies()
        {
            var isStrava = !string.IsNullOrEmpty(_profileManager.Profile.StravaID);

            chkMultiSport.Enabled = isStrava;
            chkMultiSport.Checked = isStrava && _multiSportManager.IsMultiSportDefault;
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
            var message = status.Message != null ? _translationService.GetMessage(status.Message) : string.Empty;

            var labelText = $"{status.Status ?? string.Empty} {status.Origin ?? string.Empty} {message}";

            if (status.Origin == StatusMessage.ORG_CALCULATOR)
            {
                if (status.Status == StatusMessage.STATUS_START)
                {
                    lblStatusCalculator.Text = status.Message;
                }
                else
                {
                    lblStatusCalculator.Text = labelText;

                    _calculatorStarted = true;
                }
            }
            else if (status.Origin == StatusMessage.ORG_WEBAPP)
            {
                if (status.Status == StatusMessage.STATUS_START)
                {
                    lblStatusWebApp.Text = status.Message;
                }
                else
                {
                    lblStatusWebApp.Text = labelText;

                    _webappStarted = true;
                }
            }
            else if (status.Status == StatusMessage.STATUS_PROFILESELECTED)
            {
                SetProfileDependencies();
            }

            if (_calculatorStarted && _webappStarted && !_started)
            {
                //                SetProfileDependencies();

                btnStart.Enabled = true;
                btnStart.Focus();

                _started = true;
            }
        }

        private void SplashScreen_Load(object sender, EventArgs e)
        {
            TopMost = true;
        }

        private void btnProfileManager_Click(object sender, EventArgs e)
        {
            if (_appSettings.ProfileApp != null)
            {
                var workingDirectory = Path.GetDirectoryName(_appSettings.ProfileApp);

                var startInfo = new ProcessStartInfo();

                startInfo.WorkingDirectory = workingDirectory;
                startInfo.FileName = _appSettings.ProfileApp;
                startInfo.WindowStyle = ProcessWindowStyle.Normal;

                var process = Process.Start(startInfo);

                Environment.Exit(0);
            }
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            _statusNotifier.OnStatusNotification -= OnStatusNotification;

            _multiSportManager.WriteDefaults(chkMultiSport.Checked);
            _multiSportManager.RunInMultiSportMode = chkMultiSport.Checked;
            _multiSportManager.RunWithSource = !string.IsNullOrEmpty(_profileManager.Profile.RwGpsId) ? "ridewithgps" : "strava";

            if (_multiSportManager.RunInMultiSportMode)
            {
                _translationService.MergeMessagesForLanguage("multi");
            }

            Close();

            if (_mainForm != null)
            {
                _startViewer = true;
                await _mainForm.LoadAppInWebView();
            }
        }

        private void SplashScreen_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!_startViewer)
            {
                Environment.Exit(0);
            }
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (ModifierKeys == Keys.None && keyData == Keys.Escape)
            {
                Close();

                return true;
            }

            return base.ProcessDialogKey(keyData);
        }
    }
}
