using LTC2.Desktopclients.ProfileManager.Services;
using LTC2.Shared.Messages.Interfaces;
using LTC2.Shared.Models.Desktop;
using LTC2.Shared.Models.Interprocess;
using LTC2.Shared.Repositories.Interfaces;

namespace LTC2.Desktopclients.ProfileManager.Forms
{
    public partial class ProfileManagerForm : Form
    {
        private bool _inFatalMode;

        private readonly StatusNotifier _statusNotifier;
        private readonly TesterForm _testerForm;
        private readonly IDesktopProfileRepository _desktopProfileRepository;
        private readonly ITranslationService _translationService;

        private Profile _currentProfile;
        private List<Profile> _profiles;
        private bool _edit;
        private int _keepSelectedIndex;

        public ProfileManagerForm(
            StatusNotifier statusNotifier,
            TesterForm testerForm,
            ITranslationService translationSerice,
            IDesktopProfileRepository desktopProfileRepository)
        {
            InitializeComponent();

            _statusNotifier = statusNotifier;
            _testerForm = testerForm;
            _desktopProfileRepository = desktopProfileRepository;
            _translationService = translationSerice;

            _translationService.LoadMessagesForForm(this);
        }

        private void ProfileManagerForm_Load(object sender, EventArgs e)
        {
            _statusNotifier.OnStatusNotification += OnStatusNotification;

            CheckValid();
            GetProfiles();
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

            if (status.Status == StatusMessage.STATUS_FATAL && !_inFatalMode)
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
            if (component == StatusMessage.ORG_WEBAPP)
            {
                return _translationService.GetMessage($"component.{StatusMessage.ORG_WEBAPP}");
            }
            else
            {
                return string.Empty;
            }
        }

        private void tmrKeepAlive_Tick(object sender, EventArgs e)
        {
            if (!_inFatalMode)
            {
                _statusNotifier.CheckKeepAliveStatuses();
            }
        }

        private void btnShowSecret_Click(object sender, EventArgs e)
        {
            if (txtClientSecret.PasswordChar != '*')
            {
                txtClientSecret.PasswordChar = '*';
                btnShowSecret.Text = _translationService.GetMessage("button.show.secret");
            }
            else
            {
                txtClientSecret.PasswordChar = '\0';
                btnShowSecret.Text = _translationService.GetMessage("button.hide.secret");
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            _edit = false;

            _currentProfile = new Profile()
            {
                ID = Guid.NewGuid().ToString()
            };

            EmptyDetails(true);

            txtProfileName.Focus();
        }

        private void txtClientID_TextChanged(object sender, EventArgs e)
        {
            CheckValid();
        }

        private void txtProfileName_TextChanged(object sender, EventArgs e)
        {
            CheckValid();
        }

        private void txtClientSecret_TextChanged(object sender, EventArgs e)
        {
            CheckValid();
        }

        private void CheckValid()
        {
            var clientID = txtClientID.Text.Trim();
            var isValid = true;

            foreach (var ch in clientID.ToCharArray())
            {
                if (ch < '0' || ch > '9')
                {
                    isValid = false;

                    break;
                }
            }

            if (isValid)
            {
                lblErrorClientId.Visible = false;

                isValid = txtClientID.Text != "" && txtClientSecret.Text != "" && txtProfileName.Text != "";
            }
            else
            {
                lblErrorClientId.Visible = true;
            }

            if (isValid)
            {
                btnTestProfile.Enabled = true;
            }
            else
            {
                btnTestProfile.Enabled = false;
            }

        }

        private void btnTestProfile_Click(object sender, EventArgs e)
        {
            _currentProfile.Name = txtProfileName.Text;
            _currentProfile.StravaClientSecret = txtClientSecret.Text;
            _currentProfile.StravaID = txtClientID.Text;

            _desktopProfileRepository.RemoveAllTempProfiles();

            if (_desktopProfileRepository.StoreProfile(_currentProfile, true))
            {
                _testerForm.ProfileToTest = _currentProfile.ID;
                _testerForm.ShowDialog();

                if (_testerForm.IsTestSuccessFull)
                {
                    var save = !_edit;

                    if (_edit && _currentProfile.AthleteId != _testerForm.AthleteId)
                    {
                        var parameters = new List<string>()
                        {
                            _currentProfile.AthleteId,
                            _testerForm.AthleteId
                        };

                        var dialogResult = MessageBox.Show(_translationService.GetMessage("messagebox.confirm.other.account", parameters), _translationService.GetMessage("messagebox.confirm.other.account.header"), MessageBoxButtons.YesNo);

                        save = (dialogResult == DialogResult.Yes);
                    }
                    else
                    {
                        save = true;
                    }

                    if (save)
                    {
                        _currentProfile.AthleteId = _testerForm.AthleteId;

                        _desktopProfileRepository.RemoveAllTempProfiles();
                        _desktopProfileRepository.StoreProfile(_currentProfile, false);

                        EmptyDetails(false);

                        grpDetails.Enabled = false;
                    }
                }
            }
            else
            {
                var text = _translationService.GetMessage("messagebox.error.profile");
                var caption = _translationService.GetMessage("messagebox.error.profile.header");

                MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetProfiles()
        {
            var unsortedProfiles = _desktopProfileRepository.GetProfiles();

            _profiles = unsortedProfiles.OrderBy(p => $"{p.Name} ({p.AthleteId})").ToList();

            lstProfielen.Items.Clear();

            foreach (var profile in _profiles)
            {
                lstProfielen.Items.Add($"{profile.Name} ({profile.AthleteId})");
            }

            if (_profiles.Count > 0)
            {
                var selectedIndex = _keepSelectedIndex > lstProfielen.Items.Count - 1 ? 0 : _keepSelectedIndex;

                lstProfielen.Enabled = true;
                lstProfielen.SelectedIndex = selectedIndex;
                btnEdit.Enabled = true;
                btnDelete.Enabled = true;
            }
            else
            {
                btnEdit.Enabled = false;
                lstProfielen.Enabled = false;
                btnDelete.Enabled = false;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lstProfielen.SelectedIndex >= 0)
            {
                var profile = _profiles[lstProfielen.SelectedIndex];

                var parameters = new List<string>()
                {
                    profile.Name,
                    profile.AthleteId
                };

                var dialogResult = MessageBox.Show(_translationService.GetMessage("messagebox.confirm.delete", parameters), _translationService.GetMessage("messagebox.confirm.delete.header"), MessageBoxButtons.YesNo);

                if (dialogResult == DialogResult.Yes)
                {
                    _desktopProfileRepository.RemoveAllTempProfiles();
                    _desktopProfileRepository.DeleteProfile(profile);

                    EmptyDetails(false);
                }
            }
        }

        private void EmptyDetails(bool enabled)
        {
            txtProfileName.Text = "";
            txtClientSecret.Text = "";
            txtClientID.Text = "";

            txtClientSecret.PasswordChar = '*';
            btnShowSecret.Text = "Toon Secret";

            CheckValid();
            GetProfiles();

            grpDetails.Enabled = enabled;

            CheckValid();
            GetProfiles();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (lstProfielen.SelectedIndex >= 0)
            {
                _keepSelectedIndex = lstProfielen.SelectedIndex;

                _edit = true;

                _currentProfile = _profiles[lstProfielen.SelectedIndex];

                EmptyDetails(true);

                txtProfileName.Text = _currentProfile.Name;
                txtClientID.Text = _currentProfile.StravaID;
                txtClientSecret.Text = _currentProfile.StravaClientSecret;

                txtProfileName.Focus();
            }
        }

        private void lstProfielen_DoubleClick(object sender, EventArgs e)
        {
            btnEdit_Click(sender, e);
        }
    }
}
