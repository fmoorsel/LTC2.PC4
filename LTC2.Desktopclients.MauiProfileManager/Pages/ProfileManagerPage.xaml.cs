using LTC2.Desktopclients.MauiProfileManager.Interfaces;
using LTC2.Desktopclients.MauiProfileManager.Models;
using LTC2.Desktopclients.MauiProfileManager.Services;
using LTC2.Shared.BaseMessages.Interfaces;
using LTC2.Shared.Models.Desktop;
using LTC2.Shared.Models.Interprocess;
using LTC2.Shared.Repositories.Interfaces;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LTC2.Desktopclients.MauiProfileManager.Pages
{
    public partial class ProfileManagerPage : ContentPage
    {
        private readonly AppSettings _appSettings;
        private readonly IBaseTranslationService _translationService;
        private readonly IDesktopProfileRepository _profileRepository;
        private readonly StatusNotifier _statusNotifier;
        private readonly ITesterPageFactory _testerPageFactory;

        // Strava state
        private Profile _currentProfile;
        private List<Profile> _profiles;
        private bool _edit;
        private int _keepSelectedIndex;

        // RwGPS state
        private Profile _currentProfileRwgps;
        private List<Profile> _rwgpsProfiles;
        private bool _editRwgps;
        private int _keepSelectedIndexRwgps;

        private IDispatcherTimer _timer;
        private bool _isInitializedAfterLoad;
        private bool _inFatalMode;
        private bool _webappStarted;
        private bool _isClosing;

        public ProfileManagerPage(
            AppSettings appSettings,
            StatusNotifier statusNotifier,
            ITesterPageFactory testerPageFactory,
            IDesktopProfileRepository profileRepository,
            IBaseTranslationService translationService)
        {
            _appSettings = appSettings;
            _statusNotifier = statusNotifier;
            _testerPageFactory = testerPageFactory;
            _profileRepository = profileRepository;
            _translationService = translationService;

            InitializeComponent();
            InitControls();
        }

        private void InitControls()
        {
            Title = _translationService.GetMessage("window.manage.profiles");
            BtnEditProfile.Text = _translationService.GetMessage("button.edit.profile");
            BtnNewProfile.Text = _translationService.GetMessage("button.new.profile");
            BtnDeleteProfile.Text = _translationService.GetMessage("button.delete.profile");
            BtnShowSecret.Text = _translationService.GetMessage("button.show.secret");
            BtnTestProfile.Text = _translationService.GetMessage("button.test.profile");
            LblError.Text = _translationService.GetMessage("label.error.client.id");
            LblProfileName.Text = _translationService.GetMessage("label.profile.name");
            LblClientId.Text = _translationService.GetMessage("label.strava.client.id");
            LblClientSecret.Text = _translationService.GetMessage("label.strava.client.secret");

            BtnEditProfileRwg.Text = _translationService.GetMessage("button.edit.profile");
            BtnNewProfileRwg.Text = _translationService.GetMessage("button.new.profile");
            BtnDeleteProfileRwg.Text = _translationService.GetMessage("button.delete.profile");
            BtnShowSecretRwg.Text = _translationService.GetMessage("button.show.secret");
            BtnTestProfileRwg.Text = _translationService.GetMessage("button.test.profile");
            LblProfileNameRwg.Text = _translationService.GetMessage("label.profile.name");
            LblRwGpsId.Text = _translationService.GetMessage("label.rwgps.client.id");
            LblRwGpsSecret.Text = _translationService.GetMessage("label.rwgps.client.secret");

            _timer = Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += OnTimer;
            _timer.Start();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (!_isInitializedAfterLoad)
            {
                _isInitializedAfterLoad = true;
                _statusNotifier.OnStatusNotification += OnStatusNotification;
                CheckValid();
                GetProfiles();
                CheckValidRwgps();
                GetRwGpsProfiles();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            if (!_isClosing)
            {
                _isClosing = true;
                Environment.Exit(0);
            }
            return true;
        }

        private void OnTabStrava(object sender, EventArgs e)
        {
            PanelStrava.IsVisible = true;
            PanelRwGps.IsVisible = false;
            BtnTabStrava.BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#9EA5AA");
            BtnTabRwGps.BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#C6CACD");
        }

        private void OnTabRwGps(object sender, EventArgs e)
        {
            PanelStrava.IsVisible = false;
            PanelRwGps.IsVisible = true;
            BtnTabStrava.BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#C6CACD");
            BtnTabRwGps.BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#9EA5AA");
        }

        private void OnTimer(object sender, EventArgs e)
        {
            if (!_inFatalMode) _statusNotifier.CheckKeepAliveStatuses();
        }

        private void OnStatusNotification(object sender, OnStatusMessageEventArguments e)
        {
            MainThread.InvokeOnMainThreadAsync(() => UpdateStatus(e.Status));
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
                var component = status.Origin == StatusMessage.ORG_WEBAPP
                    ? _translationService.GetMessage($"component.{StatusMessage.ORG_WEBAPP}")
                    : string.Empty;
                var text = _translationService.GetMessage("messagebox.fatal.component.exit", component);
                var caption = _translationService.GetMessage("messagebox.fatal.component.exit.header");
                await DisplayAlert(caption, text, "OK");
                Environment.Exit(0);
            }
        }

        // ===== STRAVA =====

        private void CheckValid()
        {
            var clientId = TxtClientId?.Text?.Trim() ?? string.Empty;
            var isValid = true;

            foreach (var ch in clientId)
            {
                if (ch < '0' || ch > '9') { isValid = false; break; }
            }

            if (isValid)
            {
                LblError.IsVisible = false;
                isValid = !string.IsNullOrEmpty(TxtClientId?.Text)
                       && !string.IsNullOrEmpty(TxtClientSecret?.Text)
                       && !string.IsNullOrEmpty(TxtProfileName?.Text);
            }
            else
            {
                LblError.IsVisible = true;
            }

            BtnTestProfile.IsEnabled = isValid && _webappStarted;
        }

        private void GetProfiles()
        {
            _profiles = _profileRepository.GetProfiles()
                .Where(p => !string.IsNullOrEmpty(p.StravaID))
                .OrderBy(p => $"{p.Name} ({p.AthleteId})")
                .ToList();

            var items = _profiles.Select(p => $"{p.Name} ({p.AthleteId})").ToList();
            LstProfiles.ItemsSource = null;
            LstProfiles.ItemsSource = items;

            var hasProfiles = _profiles.Count > 0;
            LstProfiles.IsEnabled = hasProfiles;
            BtnEditProfile.IsEnabled = hasProfiles;
            BtnDeleteProfile.IsEnabled = hasProfiles;

            if (hasProfiles)
            {
                var idx = _keepSelectedIndex > items.Count - 1 ? 0 : _keepSelectedIndex;
                LstProfiles.SelectedItem = idx < items.Count ? items[idx] : null;
            }
        }

        private void EmptyDetails(bool enabled)
        {
            TxtProfileName.Text = string.Empty;
            TxtClientSecret.Text = string.Empty;
            TxtClientId.Text = string.Empty;
            TxtClientSecret.IsPassword = true;
            BtnShowSecret.Text = _translationService.GetMessage("button.show.secret");
            GrpDetails.IsEnabled = enabled;
            CheckValid();
            GetProfiles();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e) => CheckValid();

        private void OnProfileSelected(object sender, SelectedItemChangedEventArgs e) { }

        private void OnNewProfile(object sender, EventArgs e)
        {
            _edit = false;
            _currentProfile = new Profile() { ID = Guid.NewGuid().ToString() };
            EmptyDetails(true);
            TxtProfileName.Focus();
        }

        private void OnEditProfile(object sender, EventArgs e)
        {
            if (LstProfiles.SelectedItem == null) return;
            var selectedText = LstProfiles.SelectedItem as string;
            var idx = _profiles.FindIndex(p => $"{p.Name} ({p.AthleteId})" == selectedText);
            if (idx >= 0)
            {
                _keepSelectedIndex = idx;
                _edit = true;
                _currentProfile = _profiles[idx];
                EmptyDetails(true);
                TxtProfileName.Text = _currentProfile.Name;
                TxtClientId.Text = _currentProfile.StravaID;
                TxtClientSecret.Text = _currentProfile.StravaClientSecret;
                TxtProfileName.Focus();
            }
        }

        private async void OnDeleteProfile(object sender, EventArgs e)
        {
            if (LstProfiles.SelectedItem == null) return;
            var selectedText = LstProfiles.SelectedItem as string;
            var idx = _profiles.FindIndex(p => $"{p.Name} ({p.AthleteId})" == selectedText);
            if (idx >= 0)
            {
                var profile = _profiles[idx];
                var text = _translationService.GetMessage("messagebox.confirm.delete", new List<string>() { profile.Name, profile.AthleteId });
                var caption = _translationService.GetMessage("messagebox.confirm.delete.header");
                var answer = await DisplayAlert(caption, text, "Ja", "Nee");
                if (answer)
                {
                    _profileRepository.RemoveAllTempProfiles();
                    _profileRepository.DeleteProfile(profile);
                    EmptyDetails(false);
                }
            }
        }

        private async void OnTestProfile(object sender, EventArgs e)
        {
            _currentProfile.Name = TxtProfileName.Text;
            _currentProfile.StravaClientSecret = TxtClientSecret.Text;
            _currentProfile.StravaID = TxtClientId.Text;

            _profileRepository.RemoveAllTempProfiles();

            if (_profileRepository.StoreProfile(_currentProfile, true))
            {
                var testerPage = _testerPageFactory.CreateTesterPage();
                testerPage.ProfileToTest = _currentProfile.ID;
                testerPage.Source = "strava";

                await Navigation.PushModalAsync(testerPage);

                if (testerPage.IsTestSuccessFull)
                {
                    var save = !_edit;
                    if (_edit && _currentProfile.AthleteId != testerPage.AthleteId)
                    {
                        var text = _translationService.GetMessage("messagebox.confirm.other.account", new List<string>() { _currentProfile.AthleteId, testerPage.AthleteId });
                        var caption = _translationService.GetMessage("messagebox.confirm.other.account.header");
                        save = await DisplayAlert(caption, text, "Ja", "Nee");
                    }
                    else save = true;

                    if (save)
                    {
                        _currentProfile.AthleteId = testerPage.AthleteId;
                        _profileRepository.RemoveAllTempProfiles();
                        _profileRepository.StoreProfile(_currentProfile, false);
                        EmptyDetails(false);
                        GrpDetails.IsEnabled = false;
                    }
                }
            }
            else
            {
                await DisplayAlert(
                    _translationService.GetMessage("messagebox.error.profile.header"),
                    _translationService.GetMessage("messagebox.error.profile"),
                    "OK");
            }
        }

        private void OnShowSecret(object sender, EventArgs e)
        {
            TxtClientSecret.IsPassword = !TxtClientSecret.IsPassword;
            BtnShowSecret.Text = TxtClientSecret.IsPassword
                ? _translationService.GetMessage("button.show.secret")
                : _translationService.GetMessage("button.hide.secret");
        }

        // ===== RIDEWITHGPS =====

        private void CheckValidRwgps()
        {
            var isValid = !string.IsNullOrEmpty(TxtRwGpsId?.Text)
                       && !string.IsNullOrEmpty(TxtRwGpsSecret?.Text)
                       && !string.IsNullOrEmpty(TxtProfileNameRwg?.Text);
            BtnTestProfileRwg.IsEnabled = isValid && _webappStarted;
        }

        private void GetRwGpsProfiles()
        {
            _rwgpsProfiles = _profileRepository.GetProfiles()
                .Where(p => !string.IsNullOrEmpty(p.RwGpsId))
                .OrderBy(p => $"{p.Name} ({p.RwGpsId})")
                .ToList();

            var items = _rwgpsProfiles.Select(p => $"{p.Name} ({p.AthleteId})").ToList();
            LstProfilesRwg.ItemsSource = null;
            LstProfilesRwg.ItemsSource = items;

            var hasProfiles = _rwgpsProfiles.Count > 0;
            LstProfilesRwg.IsEnabled = hasProfiles;
            BtnEditProfileRwg.IsEnabled = hasProfiles;
            BtnDeleteProfileRwg.IsEnabled = hasProfiles;
        }

        private void EmptyDetailsRwgps(bool enabled)
        {
            TxtProfileNameRwg.Text = string.Empty;
            TxtRwGpsSecret.Text = string.Empty;
            TxtRwGpsId.Text = string.Empty;
            TxtRwGpsSecret.IsPassword = true;
            BtnShowSecretRwg.Text = _translationService.GetMessage("button.show.secret");
            GrpDetailsRwg.IsEnabled = enabled;
            CheckValidRwgps();
            GetRwGpsProfiles();
        }

        private void OnTextChangedRwg(object sender, TextChangedEventArgs e) => CheckValidRwgps();

        private void OnProfileSelectedRwg(object sender, SelectedItemChangedEventArgs e) { }

        private void OnNewProfileRwg(object sender, EventArgs e)
        {
            _editRwgps = false;
            _currentProfileRwgps = new Profile() { ID = Guid.NewGuid().ToString() };
            EmptyDetailsRwgps(true);
            TxtProfileNameRwg.Focus();
        }

        private void OnEditProfileRwg(object sender, EventArgs e)
        {
            if (LstProfilesRwg.SelectedItem == null) return;
            var selectedText = LstProfilesRwg.SelectedItem as string;
            var idx = _rwgpsProfiles.FindIndex(p => $"{p.Name} ({p.AthleteId})" == selectedText);
            if (idx >= 0)
            {
                _keepSelectedIndexRwgps = idx;
                _editRwgps = true;
                _currentProfileRwgps = _rwgpsProfiles[idx];
                EmptyDetailsRwgps(true);
                TxtProfileNameRwg.Text = _currentProfileRwgps.Name;
                TxtRwGpsId.Text = _currentProfileRwgps.RwGpsId;
                TxtRwGpsSecret.Text = _currentProfileRwgps.RwGpsSecret;
                TxtProfileNameRwg.Focus();
            }
        }

        private async void OnDeleteProfileRwg(object sender, EventArgs e)
        {
            if (LstProfilesRwg.SelectedItem == null) return;
            var selectedText = LstProfilesRwg.SelectedItem as string;
            var idx = _rwgpsProfiles.FindIndex(p => $"{p.Name} ({p.AthleteId})" == selectedText);
            if (idx >= 0)
            {
                var profile = _rwgpsProfiles[idx];
                var text = _translationService.GetMessage("messagebox.confirm.delete", new List<string>() { profile.Name, profile.RwGpsId });
                var caption = _translationService.GetMessage("messagebox.confirm.delete.header");
                var answer = await DisplayAlert(caption, text, "Ja", "Nee");
                if (answer)
                {
                    _profileRepository.RemoveAllTempProfiles();
                    _profileRepository.DeleteProfile(profile);
                    EmptyDetailsRwgps(false);
                }
            }
        }

        private async void OnTestProfileRwg(object sender, EventArgs e)
        {
            _currentProfileRwgps.Name = TxtProfileNameRwg.Text;
            _currentProfileRwgps.RwGpsId = TxtRwGpsId.Text;
            _currentProfileRwgps.RwGpsSecret = TxtRwGpsSecret.Text;

            _profileRepository.RemoveAllTempProfiles();

            if (_profileRepository.StoreProfile(_currentProfileRwgps, true))
            {
                var testerPage = _testerPageFactory.CreateTesterPage();
                testerPage.ProfileToTest = _currentProfileRwgps.ID;
                testerPage.Source = "ridewithgps";

                await Navigation.PushModalAsync(testerPage);

                if (testerPage.IsTestSuccessFull)
                {
                    var save = !_editRwgps;
                    if (_editRwgps && _currentProfileRwgps.AthleteId != testerPage.AthleteId)
                    {
                        var text = _translationService.GetMessage("messagebox.confirm.other.account", new List<string>() { _currentProfileRwgps.AthleteId, testerPage.AthleteId });
                        var caption = _translationService.GetMessage("messagebox.confirm.other.account.header");
                        save = await DisplayAlert(caption, text, "Ja", "Nee");
                    }
                    else save = true;

                    if (save)
                    {
                        _currentProfileRwgps.AthleteId = testerPage.AthleteId;
                        _profileRepository.RemoveAllTempProfiles();
                        _profileRepository.StoreProfile(_currentProfileRwgps, false);
                        EmptyDetailsRwgps(false);
                        GrpDetailsRwg.IsEnabled = false;
                    }
                }
            }
            else
            {
                await DisplayAlert(
                    _translationService.GetMessage("messagebox.error.profile.header"),
                    _translationService.GetMessage("messagebox.error.profile"),
                    "OK");
            }
        }

        private void OnShowSecretRwg(object sender, EventArgs e)
        {
            TxtRwGpsSecret.IsPassword = !TxtRwGpsSecret.IsPassword;
            BtnShowSecretRwg.Text = TxtRwGpsSecret.IsPassword
                ? _translationService.GetMessage("button.show.secret")
                : _translationService.GetMessage("button.hide.secret");
        }
    }
}
