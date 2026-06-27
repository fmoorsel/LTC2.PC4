using Avalonia.Controls;
using Avalonia.Interactivity;
using LTC2.Desktopclients.AvaloniaClient.Services;
using LTC2.Desktopclients.AvaloniaClient.Utils;
using LTC2.Shared.BaseMessages.Interfaces;
using LTC2.Shared.Models.Desktop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LTC2.Desktopclients.AvaloniaClient.Windows
{
    public partial class SelectProfileWindow : Window
    {
        private ListBox _lstProfiles;
        private Button _btnSelectProfile;
        private Button _btnProfileManager;

        private TextBlock _lblLine1;
        private TextBlock _lblLine2;

        private bool _isLoaded;
        private bool _hasSelectedProfile;

        private readonly ProfileManager _profileManager;
        private readonly IBaseTranslationService _translationService;
        private readonly ProfileManagerStarter _profileManagerStarter;

        private List<Profile> _profiles;

        public SelectProfileWindow()
        {
        }

        public SelectProfileWindow(
            ProfileManager profileManager,
            IBaseTranslationService translationService,
            ProfileManagerStarter profileManagerStarter) : this()
        {
            _profileManager = profileManager;
            _translationService = translationService;
            _profileManagerStarter = profileManagerStarter;

            InitializeComponent();
            InitControls();

            this.Activated += OnActivated;
            this.Closing += OnClose;
        }

        private void InitControls()
        {
            _lblLine1 = this.FindControl<TextBlock>("LblLine1");
            _lblLine2 = this.FindControl<TextBlock>("LblLine2");

            _lstProfiles = this.FindControl<ListBox>("LstProfiles");
            _btnSelectProfile = this.FindControl<Button>("BtnSelectProfile");
            _btnProfileManager = this.FindControl<Button>("BtnProfileManager");

            TryTranslate(this, "title.selectprofile.window");
            TryTranslate(_btnSelectProfile, "button.selectprofile.select");
            TryTranslate(_btnProfileManager, "button.selectprofile.profilemanager");

            var lblSelectProfile = this.FindControl<TextBlock>("LblSelectProfile");
            TryTranslate(lblSelectProfile, "label.selectprofile.header");

            PopulateProfiles();
        }

        private void TryTranslate(Control control, string key)
        {
            var message = _translationService.GetMessage(key);

            if (control is TextBlock textBlock) textBlock.Text = message;
            else if (control is Button button) button.Content = message;
            else if (control is Window window) window.Title = message;
        }

        private void PopulateProfiles()
        {
            var profiles = _profileManager.GetProfiles();
            var items = new ObservableCollection<string>(profiles.Select(p => p.Name ?? p.ID));

            _lstProfiles.ItemsSource = items;

            if (items.Count > 0)
            {
                _lstProfiles.SelectedIndex = 0;
            }
        }

        public void ClickHandlerProfileDoubleTapped(object sender, Avalonia.Input.TappedEventArgs e)
        {
            ClickHandlerSelectProfile(sender, new RoutedEventArgs());
        }

        public void ClickHandlerSelectProfile(object sender, RoutedEventArgs e)
        {
            if (_lstProfiles.SelectedIndex >= 0)
            {
                _hasSelectedProfile = true;

                _profileManager.Profile = _profiles[_lstProfiles.SelectedIndex];

                Close();
            }
        }

        public void ClickHandlerProfileManager(object sender, RoutedEventArgs e)
        {
            _profileManagerStarter.Start();
        }

        private void OnClose(object sender, EventArgs e)
        {
            if (!_hasSelectedProfile)
            {
                Environment.Exit(0);
            }
        }

        private void OnActivated(object sender, EventArgs e)
        {
            if (!_isLoaded)
            {
                _profiles = _profileManager.GetProfiles().OrderBy(p => $"{p.Name} - ({p.AthleteId})").ToList();
                var hasProfiles = _profiles.Count == 0;

                _isLoaded = true;

                if (hasProfiles)
                {
                    _lstProfiles.IsEnabled = false;
                    _btnSelectProfile.IsEnabled = false;

                    _lblLine1.Text = _translationService.GetMessage("label.no.profiles.1");
                    _lblLine2.Text = _translationService.GetMessage("label.no.profiles.2");
                }
                else
                {
                    _btnSelectProfile.IsEnabled = true;

                    _lblLine1.Text = _translationService.GetMessage("label.multiple.profiles.1");
                    _lblLine2.Text = _translationService.GetMessage("label.multiple.profiles.2");

                    var items = new ObservableCollection<string>();
                    foreach (var profile in _profiles)
                    {
                        items.Add($"{profile.Name} - ({profile.AthleteId})");
                    }

                    _lstProfiles.ItemsSource = items;
                    _lstProfiles.SelectedIndex = 0;
                }

            }
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            WindowUtilities.RemoveMinimizeButton(this);
        }
    }
}
