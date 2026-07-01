using LTC2.Desktopclients.MauiClient.Services;
using LTC2.Shared.BaseMessages.Interfaces;
using LTC2.Shared.Models.Desktop;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LTC2.Desktopclients.MauiClient.Pages
{
    public partial class SelectProfilePage : ContentPage
    {
        private readonly ProfileManager _profileManager;
        private readonly IBaseTranslationService _translationService;
        private readonly ProfileManagerStarter _profileManagerStarter;

        private List<Profile> _profiles;
        private bool _hasSelectedProfile;
        private readonly TaskCompletionSource<bool> _closeTcs = new TaskCompletionSource<bool>();

        public SelectProfilePage(
            ProfileManager profileManager,
            IBaseTranslationService translationService,
            ProfileManagerStarter profileManagerStarter)
        {
            _profileManager = profileManager;
            _translationService = translationService;
            _profileManagerStarter = profileManagerStarter;

            InitializeComponent();

            Title = _translationService.GetMessage("title.selectprofile.window");
            BtnSelectProfile.Text = _translationService.GetMessage("button.selectprofile.select");
            BtnProfileManager.Text = _translationService.GetMessage("button.selectprofile.profilemanager");

            LoadProfiles();
        }

        private void LoadProfiles()
        {
            _profiles = _profileManager.GetProfiles().OrderBy(p => $"{p.Name} - ({p.AthleteId})").ToList();

            if (_profiles.Count == 0)
            {
                LstProfiles.IsEnabled = false;
                BtnSelectProfile.IsEnabled = false;
                LblLine1.Text = _translationService.GetMessage("label.no.profiles.1");
                LblLine2.Text = _translationService.GetMessage("label.no.profiles.2");
            }
            else
            {
                BtnSelectProfile.IsEnabled = true;
                LblLine1.Text = _translationService.GetMessage("label.multiple.profiles.1");
                LblLine2.Text = _translationService.GetMessage("label.multiple.profiles.2");
                LstProfiles.ItemsSource = _profiles.Select(p => $"{p.Name} - ({p.AthleteId})").ToList();
                if (_profiles.Count > 0) LstProfiles.SelectedItem = LstProfiles.ItemsSource is System.Collections.IList list && list.Count > 0 ? list[0] : null;
            }
        }

        public Task WaitForClose() => _closeTcs.Task;

        private async void OnSelectProfile(object sender, EventArgs e)
        {
            var idx = LstProfiles.SelectedItem != null
                ? _profiles.FindIndex(p => $"{p.Name} - ({p.AthleteId})" == LstProfiles.SelectedItem as string)
                : (_profiles.Count > 0 ? 0 : -1);

            if (idx >= 0)
            {
                _hasSelectedProfile = true;
                _profileManager.Profile = _profiles[idx];
                _closeTcs.TrySetResult(true);
                await Navigation.PopModalAsync();
            }
        }

        private void OnProfileManager(object sender, EventArgs e)
        {
            _profileManagerStarter.Start();
        }

        protected override bool OnBackButtonPressed()
        {
            if (!_hasSelectedProfile)
            {
                _closeTcs.TrySetResult(false);
                Environment.Exit(0);
            }
            return true;
        }
    }
}
