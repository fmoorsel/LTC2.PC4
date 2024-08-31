using LTC2.Desktopclients.WindowsClient.Models;
using LTC2.Desktopclients.WindowsClient.Services;
using LTC2.Shared.Messages.Interfaces;
using LTC2.Shared.Models.Desktop;
using System.Diagnostics;

namespace LTC2.Desktopclients.WindowsClient.Forms
{
    public partial class SelectProfileForm : Form
    {
        public bool ShouldProgress { get; set; }

        private readonly ProfileManager _profileManager;
        private readonly AppSettings _appSettings;
        private readonly ITranslationService _translationService;

        private List<Profile> _profiles;

        public SelectProfileForm(
            ProfileManager profileManager,
            ITranslationService translationService,
            AppSettings appSettings)
        {
            InitializeComponent();

            _profileManager = profileManager;
            _appSettings = appSettings;
            _translationService = translationService;

            _translationService.LoadMessagesForForm(this);
        }

        private void SelectProfileForm_Load(object sender, EventArgs e)
        {
            _profiles = _profileManager.GetProfiles().OrderBy(p => $"{p.Name} - ({p.AthleteId})").ToList();
            var hasProfiles = _profiles.Count == 0;

            if (hasProfiles)
            {
                grpSelectProfile.Enabled = false;
                btnManage.Visible = true;

                lblInstruction1.Text = _translationService.GetMessage("label.no.profiles.1");
                lblInstruction2.Text = _translationService.GetMessage("label.no.profiles.2");
            }
            else
            {
                grpSelectProfile.Enabled = true;
                btnManage.Visible = true;

                lblInstruction1.Text = _translationService.GetMessage("label.multiple.profiles.1");
                lblInstruction2.Text = _translationService.GetMessage("label.multiple.profiles.2");

                foreach (var profile in _profiles)
                {
                    lstProfiles.Items.Add($"{profile.Name} - ({profile.AthleteId})");
                }

                lstProfiles.SelectedIndex = 0;
            }

            ShouldProgress = true;
        }

        private void btnSelectProfiles_Click(object sender, EventArgs e)
        {
            if (lstProfiles.SelectedIndex >= 0)
            {
                ShouldProgress = false;

                _profileManager.Profile = _profiles[lstProfiles.SelectedIndex];

                Close();
            }
        }

        private void lstProfiles_DoubleClick(object sender, EventArgs e)
        {
            btnSelectProfiles_Click(sender, e);
        }

        private void btnManage_Click(object sender, EventArgs e)
        {
            if (_appSettings.ProfileApp != null)
            {
                var workingDirectory = Path.GetDirectoryName(_appSettings.ProfileApp);

                var startInfo = new ProcessStartInfo();

                startInfo.WorkingDirectory = workingDirectory;
                startInfo.FileName = _appSettings.ProfileApp;
                startInfo.WindowStyle = ProcessWindowStyle.Normal;

                var process = Process.Start(startInfo);

                Close();
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
