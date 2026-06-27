using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LTC2.Desktopclients.AvaloniaProfileManager.Interfaces;
using LTC2.Desktopclients.AvaloniaProfileManager.Models;
using LTC2.Desktopclients.AvaloniaProfileManager.Services;
using LTC2.Shared.BaseMessages.Interfaces;
using LTC2.Shared.Models.Desktop;
using LTC2.Shared.Models.Interprocess;
using LTC2.Shared.Repositories.Interfaces;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LTC2.Desktopclients.AvaloniaProfileManager.Windows
{
    public partial class ProfileManagerWindow : Window
    {
        private readonly AppSettings _appSettings;
        private readonly IBaseTranslationService _translationService;
        private readonly IDesktopProfileRepository _profileRepository;
        private readonly StatusNotifier _statusNotifier;
        private readonly ITesterWindowFactory _testerWindowFactory;

        // Strava controls
        private Button _btnEditProfile;
        private Button _btnNewProfile;
        private Button _btnDeleteProfile;
        private Button _btnTestProfile;
        private Button _btnShowSecret;
        private TextBlock _lblProfileName;
        private TextBlock _lblClientId;
        private TextBlock _lblClientSecret;
        private TextBlock _lblError;
        private TextBox _txtProfileName;
        private TextBox _txtClientId;
        private TextBox _txtClientSecret;
        private ListBox _lstProfiles;
        private Border _grpDetails;

        // Strava state
        private Profile _currentProfile;
        private List<Profile> _profiles;
        private bool _edit;
        private int _keepSelectedIndex;

        // RwGPS controls
        private Button _btnEditProfileRwg;
        private Button _btnNewProfileRwg;
        private Button _btnDeleteProfileRwg;
        private Button _btnTestProfileRwg;
        private Button _btnShowSecretRwg;
        private TextBlock _lblProfileNameRwg;
        private TextBlock _lblRwGpsId;
        private TextBlock _lblRwGpsSecret;
        private TextBox _txtProfileNameRwg;
        private TextBox _txtRwGpsId;
        private TextBox _txtRwGpsSecret;
        private ListBox _lstProfilesRwg;
        private Border _grpDetailsRwg;

        // RwGPS state
        private Profile _currentProfileRwgps;
        private List<Profile> _rwgpsProfiles;
        private bool _editRwgps;
        private int _keepSelectedIndexRwgps;

        private DispatcherTimer _timer;
        private bool _isInitializedAfterLoad;
        private bool _inFatalMode;
        private bool _webappStarted;
        private bool _isClosing;

        public ProfileManagerWindow()
        {
        }

        public ProfileManagerWindow(
            AppSettings appSettings,
            StatusNotifier statusNotifier,
            ITesterWindowFactory testerWindowFactory,
            IDesktopProfileRepository profileRepository,
            IBaseTranslationService translationService)
        {
            _appSettings = appSettings;
            _statusNotifier = statusNotifier;
            _testerWindowFactory = testerWindowFactory;
            _profileRepository = profileRepository;
            _translationService = translationService;

            InitializeComponent();
            InitControls();

            this.Closing += OnClose;
            this.Activated += OnActivated;
        }

        private void InitControls()
        {
            // Strava
            _btnEditProfile = this.FindControl<Button>("BtnEditProfile");
            _btnNewProfile = this.FindControl<Button>("BtnNewProfile");
            _btnDeleteProfile = this.FindControl<Button>("BtnDeleteProfile");
            _btnTestProfile = this.FindControl<Button>("BtnTestProfile");
            _btnShowSecret = this.FindControl<Button>("BtnShowSecret");
            _lblProfileName = this.FindControl<TextBlock>("LblProfileName");
            _lblClientId = this.FindControl<TextBlock>("LblClientId");
            _lblClientSecret = this.FindControl<TextBlock>("LblClientSecret");
            _lblError = this.FindControl<TextBlock>("LblError");
            _txtProfileName = this.FindControl<TextBox>("TxtProfileName");
            _txtClientId = this.FindControl<TextBox>("TxtClientId");
            _txtClientSecret = this.FindControl<TextBox>("TxtClientSecret");
            _lstProfiles = this.FindControl<ListBox>("LstProfiles");
            _grpDetails = this.FindControl<Border>("GrpDetails");

            // RwGPS
            _btnEditProfileRwg = this.FindControl<Button>("BtnEditProfileRwg");
            _btnNewProfileRwg = this.FindControl<Button>("BtnNewProfileRwg");
            _btnDeleteProfileRwg = this.FindControl<Button>("BtnDeleteProfileRwg");
            _btnTestProfileRwg = this.FindControl<Button>("BtnTestProfileRwg");
            _btnShowSecretRwg = this.FindControl<Button>("BtnShowSecretRwg");
            _lblProfileNameRwg = this.FindControl<TextBlock>("LblProfileNameRwg");
            _lblRwGpsId = this.FindControl<TextBlock>("LblRwGpsId");
            _lblRwGpsSecret = this.FindControl<TextBlock>("LblRwGpsSecret");
            _txtProfileNameRwg = this.FindControl<TextBox>("TxtProfileNameRwg");
            _txtRwGpsId = this.FindControl<TextBox>("TxtRwGpsId");
            _txtRwGpsSecret = this.FindControl<TextBox>("TxtRwGpsSecret");
            _lstProfilesRwg = this.FindControl<ListBox>("LstProfilesRwg");
            _grpDetailsRwg = this.FindControl<Border>("GrpDetailsRwg");

            // Wire events - Strava
            if (_lstProfiles != null) _lstProfiles.DoubleTapped += OnDoubleClick;
            if (_txtClientId != null) _txtClientId.TextChanged += OnTextChanged;
            if (_txtClientSecret != null) _txtClientSecret.TextChanged += OnTextChanged;
            if (_txtProfileName != null) _txtProfileName.TextChanged += OnTextChanged;
            if (_btnNewProfile != null) _btnNewProfile.Click += OnNewProfile;
            if (_btnDeleteProfile != null) _btnDeleteProfile.Click += OnDeleteProfile;
            if (_btnEditProfile != null) _btnEditProfile.Click += OnEditProfile;

            // Wire events - RwGPS
            if (_lstProfilesRwg != null) _lstProfilesRwg.DoubleTapped += OnDoubleClickRwg;
            if (_txtRwGpsId != null) _txtRwGpsId.TextChanged += OnTextChangedRwg;
            if (_txtRwGpsSecret != null) _txtRwGpsSecret.TextChanged += OnTextChangedRwg;
            if (_txtProfileNameRwg != null) _txtProfileNameRwg.TextChanged += OnTextChangedRwg;
            if (_btnNewProfileRwg != null) _btnNewProfileRwg.Click += OnNewProfileRwg;
            if (_btnDeleteProfileRwg != null) _btnDeleteProfileRwg.Click += OnDeleteProfileRwg;
            if (_btnEditProfileRwg != null) _btnEditProfileRwg.Click += OnEditProfileRwg;

            // Translate labels - Strava
            TryTranslate(this, "window.manage.profiles");
            TryTranslate(_btnEditProfile, "button.edit.profile");
            TryTranslate(_btnNewProfile, "button.new.profile");
            TryTranslate(_btnDeleteProfile, "button.delete.profile");
            TryTranslate(_btnShowSecret, "button.show.secret");
            TryTranslate(_btnTestProfile, "button.test.profile");
            TryTranslate(_lblError, "label.error.client.id");
            TryTranslate(_lblProfileName, "label.profile.name");
            TryTranslate(_lblClientId, "label.strava.client.id");
            TryTranslate(_lblClientSecret, "label.strava.client.secret");

            // Translate labels - RwGPS
            TryTranslate(_btnEditProfileRwg, "button.edit.profile");
            TryTranslate(_btnNewProfileRwg, "button.new.profile");
            TryTranslate(_btnDeleteProfileRwg, "button.delete.profile");
            TryTranslate(_btnShowSecretRwg, "button.show.secret");
            TryTranslate(_btnTestProfileRwg, "button.test.profile");
            TryTranslate(_lblProfileNameRwg, "label.profile.name");
            TryTranslate(_lblRwGpsId, "label.rwgps.client.id");
            TryTranslate(_lblRwGpsSecret, "label.rwgps.client.secret");

            _timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += OnTimer;
            _timer.Start();
        }

        private void TryTranslate(Control control, string key)
        {
            if (control == null) return;
            var msg = _translationService.GetMessage(key);
            if (control is TextBlock tb) tb.Text = msg;
            else if (control is Button btn) btn.Content = msg;
            else if (control is Window wnd) wnd.Title = msg;
        }

        private void OnActivated(object sender, EventArgs e)
        {
            if (!_isInitializedAfterLoad)
            {
                _isInitializedAfterLoad = true;
                OnLoad();
            }
        }

        private void OnLoad()
        {
            _statusNotifier.OnStatusNotification += OnStatusNotification;

            CheckValid();
            GetProfiles();
            CheckValidRwgps();
            GetRwGpsProfiles();
        }

        private void OnClose(object sender, WindowClosingEventArgs args)
        {
            if (!_isClosing)
            {
                args.Cancel = true;
                _isClosing = true;
                Dispatcher.UIThread.Post(() => Environment.Exit(0));
            }
        }

        private void OnTimer(object sender, EventArgs e)
        {
            if (!_inFatalMode)
            {
                _statusNotifier.CheckKeepAliveStatuses();
            }
        }

        private void OnStatusNotification(object sender, OnStatusMessageEventArguments e)
        {
            Dispatcher.UIThread.Post(() => UpdateStatus(e.Status));
        }

        private async void UpdateStatus(StatusMessage status)
        {
            if (status.Status == StatusMessage.STATUS_PING)
            {
                _webappStarted = true;
            }
            else if (status.Status == StatusMessage.STATUS_FATAL && !_inFatalMode)
            {
                _inFatalMode = true;

                var component = GetComponentNiceName(status.Origin);
                var text = _translationService.GetMessage("messagebox.fatal.component.exit", component);
                var caption = _translationService.GetMessage("messagebox.fatal.component.exit.header");

                var box = MessageBoxManager.GetMessageBoxStandard(caption, text, ButtonEnum.Ok);
                await box.ShowWindowDialogAsync(this);

                Environment.Exit(0);
            }
        }

        private string GetComponentNiceName(string component)
        {
            return component == StatusMessage.ORG_WEBAPP
                ? _translationService.GetMessage($"component.{StatusMessage.ORG_WEBAPP}")
                : string.Empty;
        }

        // ===== STRAVA =====

        private void CheckValid()
        {
            var clientId = _txtClientId?.Text?.Trim() ?? string.Empty;
            var isValid = true;

            foreach (var ch in clientId)
            {
                if (ch < '0' || ch > '9')
                {
                    isValid = false;
                    break;
                }
            }

            if (isValid)
            {
                if (_lblError != null) _lblError.IsVisible = false;
                isValid = !string.IsNullOrEmpty(_txtClientId?.Text)
                       && !string.IsNullOrEmpty(_txtClientSecret?.Text)
                       && !string.IsNullOrEmpty(_txtProfileName?.Text);
            }
            else
            {
                if (_lblError != null) _lblError.IsVisible = true;
            }

            if (_btnTestProfile != null)
            {
                _btnTestProfile.IsEnabled = isValid && _webappStarted;
            }
        }

        private void GetProfiles()
        {
            _profiles = _profileRepository.GetProfiles()
                .Where(p => !string.IsNullOrEmpty(p.StravaID))
                .OrderBy(p => $"{p.Name} ({p.AthleteId})")
                .ToList();

            _lstProfiles?.Items.Clear();

            foreach (var profile in _profiles)
            {
                _lstProfiles?.Items.Add($"{profile.Name} ({profile.AthleteId})");
            }

            if (_profiles.Count > 0)
            {
                var selectedIndex = _keepSelectedIndex > _lstProfiles.Items.Count - 1 ? 0 : _keepSelectedIndex;
                if (_lstProfiles != null)
                {
                    _lstProfiles.IsEnabled = true;
                    _lstProfiles.SelectedIndex = selectedIndex;
                }
                if (_btnEditProfile != null) _btnEditProfile.IsEnabled = true;
                if (_btnDeleteProfile != null) _btnDeleteProfile.IsEnabled = true;
            }
            else
            {
                if (_lstProfiles != null) _lstProfiles.IsEnabled = false;
                if (_btnEditProfile != null) _btnEditProfile.IsEnabled = false;
                if (_btnDeleteProfile != null) _btnDeleteProfile.IsEnabled = false;
            }
        }

        private void EmptyDetails(bool enabled)
        {
            if (_txtProfileName != null) _txtProfileName.Text = string.Empty;
            if (_txtClientSecret != null) _txtClientSecret.Text = string.Empty;
            if (_txtClientId != null) _txtClientId.Text = string.Empty;
            if (_txtClientSecret != null) _txtClientSecret.PasswordChar = '*';
            if (_btnShowSecret != null) _btnShowSecret.Content = _translationService.GetMessage("button.show.secret");

            if (_grpDetails != null) _grpDetails.IsEnabled = enabled;

            CheckValid();
            GetProfiles();
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            CheckValid();
        }

        private void OnDoubleClick(object sender, EventArgs e)
        {
            OnEditProfile(sender, e);
        }

        private void OnNewProfile(object sender, EventArgs e)
        {
            _edit = false;
            _currentProfile = new Profile() { ID = Guid.NewGuid().ToString() };
            EmptyDetails(true);
            _txtProfileName?.Focus();
        }

        private void OnEditProfile(object sender, EventArgs e)
        {
            if (_lstProfiles?.SelectedIndex >= 0)
            {
                _keepSelectedIndex = _lstProfiles.SelectedIndex;
                _edit = true;
                _currentProfile = _profiles[_lstProfiles.SelectedIndex];

                EmptyDetails(true);

                if (_txtProfileName != null) _txtProfileName.Text = _currentProfile.Name;
                if (_txtClientId != null) _txtClientId.Text = _currentProfile.StravaID;
                if (_txtClientSecret != null) _txtClientSecret.Text = _currentProfile.StravaClientSecret;

                _txtProfileName?.Focus();
            }
        }

        private async void OnDeleteProfile(object sender, EventArgs e)
        {
            if (_lstProfiles?.SelectedIndex >= 0)
            {
                var profile = _profiles[_lstProfiles.SelectedIndex];

                var parameters = new List<string>() { profile.Name, profile.AthleteId };
                var text = _translationService.GetMessage("messagebox.confirm.delete", parameters);
                var caption = _translationService.GetMessage("messagebox.confirm.delete.header");
                var box = MessageBoxManager.GetMessageBoxStandard(caption, text, ButtonEnum.YesNo);
                var answer = await box.ShowWindowDialogAsync(this);

                if (answer == ButtonResult.Yes)
                {
                    _profileRepository.RemoveAllTempProfiles();
                    _profileRepository.DeleteProfile(profile);
                    EmptyDetails(false);
                }
            }
        }

        public async void ClickTestHandler(object sender, RoutedEventArgs e)
        {
            _currentProfile.Name = _txtProfileName.Text;
            _currentProfile.StravaClientSecret = _txtClientSecret.Text;
            _currentProfile.StravaID = _txtClientId.Text;

            _profileRepository.RemoveAllTempProfiles();

            if (_profileRepository.StoreProfile(_currentProfile, true))
            {
                var testerWindow = _testerWindowFactory.CreateTesterWindow();
                testerWindow.ProfileToTest = _currentProfile.ID;
                testerWindow.Source = "strava";

                await testerWindow.TestProfile(this);

                if (testerWindow.IsTestSuccessFull)
                {
                    var save = !_edit;

                    if (_edit && _currentProfile.AthleteId != testerWindow.AthleteId)
                    {
                        var parameters = new List<string>() { _currentProfile.AthleteId, testerWindow.AthleteId };
                        var text = _translationService.GetMessage("messagebox.confirm.other.account", parameters);
                        var caption = _translationService.GetMessage("messagebox.confirm.other.account.header");
                        var box = MessageBoxManager.GetMessageBoxStandard(caption, text, ButtonEnum.YesNo);
                        var answer = await box.ShowWindowDialogAsync(this);
                        save = answer == ButtonResult.Yes;
                    }
                    else
                    {
                        save = true;
                    }

                    if (save)
                    {
                        _currentProfile.AthleteId = testerWindow.AthleteId;
                        _profileRepository.RemoveAllTempProfiles();
                        _profileRepository.StoreProfile(_currentProfile, false);
                        EmptyDetails(false);
                        if (_grpDetails != null) _grpDetails.IsEnabled = false;
                    }
                }
            }
            else
            {
                var text = _translationService.GetMessage("messagebox.error.profile");
                var caption = _translationService.GetMessage("messagebox.error.profile.header");
                var box = MessageBoxManager.GetMessageBoxStandard(caption, text, ButtonEnum.Ok);
                await box.ShowWindowDialogAsync(this);
            }
        }

        public void ClickShowSecretHandler(object sender, RoutedEventArgs e)
        {
            if (_txtClientSecret.PasswordChar != '*')
            {
                _txtClientSecret.PasswordChar = '*';
                if (_btnShowSecret != null) _btnShowSecret.Content = _translationService.GetMessage("button.show.secret");
            }
            else
            {
                _txtClientSecret.PasswordChar = '\0';
                if (_btnShowSecret != null) _btnShowSecret.Content = _translationService.GetMessage("button.hide.secret");
            }
        }

        // ===== RIDEWITHGPS =====

        private void CheckValidRwgps()
        {
            var isValid = !string.IsNullOrEmpty(_txtRwGpsId?.Text)
                       && !string.IsNullOrEmpty(_txtRwGpsSecret?.Text)
                       && !string.IsNullOrEmpty(_txtProfileNameRwg?.Text);

            if (_btnTestProfileRwg != null)
            {
                _btnTestProfileRwg.IsEnabled = isValid && _webappStarted;
            }
        }

        private void GetRwGpsProfiles()
        {
            _rwgpsProfiles = _profileRepository.GetProfiles()
                .Where(p => !string.IsNullOrEmpty(p.RwGpsId))
                .OrderBy(p => $"{p.Name} ({p.RwGpsId})")
                .ToList();

            _lstProfilesRwg?.Items.Clear();

            foreach (var profile in _rwgpsProfiles)
            {
                _lstProfilesRwg?.Items.Add($"{profile.Name} ({profile.AthleteId})");
            }

            if (_rwgpsProfiles.Count > 0)
            {
                var selectedIndex = _keepSelectedIndexRwgps > _lstProfilesRwg.Items.Count - 1 ? 0 : _keepSelectedIndexRwgps;
                if (_lstProfilesRwg != null)
                {
                    _lstProfilesRwg.IsEnabled = true;
                    _lstProfilesRwg.SelectedIndex = selectedIndex;
                }
                if (_btnEditProfileRwg != null) _btnEditProfileRwg.IsEnabled = true;
                if (_btnDeleteProfileRwg != null) _btnDeleteProfileRwg.IsEnabled = true;
            }
            else
            {
                if (_lstProfilesRwg != null) _lstProfilesRwg.IsEnabled = false;
                if (_btnEditProfileRwg != null) _btnEditProfileRwg.IsEnabled = false;
                if (_btnDeleteProfileRwg != null) _btnDeleteProfileRwg.IsEnabled = false;
            }
        }

        private void EmptyDetailsRwgps(bool enabled)
        {
            if (_txtProfileNameRwg != null) _txtProfileNameRwg.Text = string.Empty;
            if (_txtRwGpsSecret != null) _txtRwGpsSecret.Text = string.Empty;
            if (_txtRwGpsId != null) _txtRwGpsId.Text = string.Empty;
            if (_txtRwGpsSecret != null) _txtRwGpsSecret.PasswordChar = '*';
            if (_btnShowSecretRwg != null) _btnShowSecretRwg.Content = _translationService.GetMessage("button.show.secret");

            if (_grpDetailsRwg != null) _grpDetailsRwg.IsEnabled = enabled;

            CheckValidRwgps();
            GetRwGpsProfiles();
        }

        private void OnTextChangedRwg(object sender, EventArgs e)
        {
            CheckValidRwgps();
        }

        private void OnDoubleClickRwg(object sender, EventArgs e)
        {
            OnEditProfileRwg(sender, e);
        }

        private void OnNewProfileRwg(object sender, EventArgs e)
        {
            _editRwgps = false;
            _currentProfileRwgps = new Profile() { ID = Guid.NewGuid().ToString() };
            EmptyDetailsRwgps(true);
            _txtProfileNameRwg?.Focus();
        }

        private void OnEditProfileRwg(object sender, EventArgs e)
        {
            if (_lstProfilesRwg?.SelectedIndex >= 0)
            {
                _keepSelectedIndexRwgps = _lstProfilesRwg.SelectedIndex;
                _editRwgps = true;
                _currentProfileRwgps = _rwgpsProfiles[_lstProfilesRwg.SelectedIndex];

                EmptyDetailsRwgps(true);

                if (_txtProfileNameRwg != null) _txtProfileNameRwg.Text = _currentProfileRwgps.Name;
                if (_txtRwGpsId != null) _txtRwGpsId.Text = _currentProfileRwgps.RwGpsId;
                if (_txtRwGpsSecret != null) _txtRwGpsSecret.Text = _currentProfileRwgps.RwGpsSecret;

                _txtProfileNameRwg?.Focus();
            }
        }

        private async void OnDeleteProfileRwg(object sender, EventArgs e)
        {
            if (_lstProfilesRwg?.SelectedIndex >= 0)
            {
                var profile = _rwgpsProfiles[_lstProfilesRwg.SelectedIndex];

                var parameters = new List<string>() { profile.Name, profile.RwGpsId };
                var text = _translationService.GetMessage("messagebox.confirm.delete", parameters);
                var caption = _translationService.GetMessage("messagebox.confirm.delete.header");
                var box = MessageBoxManager.GetMessageBoxStandard(caption, text, ButtonEnum.YesNo);
                var answer = await box.ShowWindowDialogAsync(this);

                if (answer == ButtonResult.Yes)
                {
                    _profileRepository.RemoveAllTempProfiles();
                    _profileRepository.DeleteProfile(profile);
                    EmptyDetailsRwgps(false);
                }
            }
        }

        public async void ClickTestHandlerRwg(object sender, RoutedEventArgs e)
        {
            _currentProfileRwgps.Name = _txtProfileNameRwg.Text;
            _currentProfileRwgps.RwGpsId = _txtRwGpsId.Text;
            _currentProfileRwgps.RwGpsSecret = _txtRwGpsSecret.Text;

            _profileRepository.RemoveAllTempProfiles();

            if (_profileRepository.StoreProfile(_currentProfileRwgps, true))
            {
                var testerWindow = _testerWindowFactory.CreateTesterWindow();
                testerWindow.ProfileToTest = _currentProfileRwgps.ID;
                testerWindow.Source = "ridewithgps";

                await testerWindow.TestProfile(this);

                if (testerWindow.IsTestSuccessFull)
                {
                    var save = !_editRwgps;

                    if (_editRwgps && _currentProfileRwgps.AthleteId != testerWindow.AthleteId)
                    {
                        var parameters = new List<string>() { _currentProfileRwgps.AthleteId, testerWindow.AthleteId };
                        var text = _translationService.GetMessage("messagebox.confirm.other.account", parameters);
                        var caption = _translationService.GetMessage("messagebox.confirm.other.account.header");
                        var box = MessageBoxManager.GetMessageBoxStandard(caption, text, ButtonEnum.YesNo);
                        var answer = await box.ShowWindowDialogAsync(this);
                        save = answer == ButtonResult.Yes;
                    }
                    else
                    {
                        save = true;
                    }

                    if (save)
                    {
                        _currentProfileRwgps.AthleteId = testerWindow.AthleteId;
                        _profileRepository.RemoveAllTempProfiles();
                        _profileRepository.StoreProfile(_currentProfileRwgps, false);
                        EmptyDetailsRwgps(false);
                        if (_grpDetailsRwg != null) _grpDetailsRwg.IsEnabled = false;
                    }
                }
            }
            else
            {
                var text = _translationService.GetMessage("messagebox.error.profile");
                var caption = _translationService.GetMessage("messagebox.error.profile.header");
                var box = MessageBoxManager.GetMessageBoxStandard(caption, text, ButtonEnum.Ok);
                await box.ShowWindowDialogAsync(this);
            }
        }

        public void ClickShowSecretHandlerRwg(object sender, RoutedEventArgs e)
        {
            if (_txtRwGpsSecret.PasswordChar != '*')
            {
                _txtRwGpsSecret.PasswordChar = '*';
                if (_btnShowSecretRwg != null) _btnShowSecretRwg.Content = _translationService.GetMessage("button.show.secret");
            }
            else
            {
                _txtRwGpsSecret.PasswordChar = '\0';
                if (_btnShowSecretRwg != null) _btnShowSecretRwg.Content = _translationService.GetMessage("button.hide.secret");
            }
        }
    }
}
