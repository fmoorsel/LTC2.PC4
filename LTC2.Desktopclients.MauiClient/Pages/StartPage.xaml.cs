using LTC2.Desktopclients.MauiClient.Services;
using LTC2.Shared.BaseMessages.Interfaces;
using LTC2.Shared.Models.Interprocess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;

namespace LTC2.Desktopclients.MauiClient.Pages
{
    public partial class StartPage : ContentPage
    {
        private readonly IServiceProvider _services;
        private readonly StatusNotifier _statusNotifier;
        private readonly IBaseTranslationService _translationService;
        private readonly ProfileManagerStarter _profileManagerStarter;
        private readonly MultiSportManager _multiSportManager;
        private readonly ProfileManager _profileManager;

        private bool _started;
        private bool _startViewer;
        private bool _calculatorStarted;
        private bool _webappStarted;
        private bool _closing;

        public StartPage(
            IServiceProvider services,
            StatusNotifier statusNotifier,
            ProfileManagerStarter profileManagerStarter,
            ProfileManager profileManager,
            MultiSportManager multiSportManager,
            IBaseTranslationService translationService)
        {
            _services = services;
            _statusNotifier = statusNotifier;
            _translationService = translationService;
            _profileManagerStarter = profileManagerStarter;
            _multiSportManager = multiSportManager;
            _profileManager = profileManager;

            InitializeComponent();

            Title = _translationService.GetMessage("title.start.window");
            LblLabelStatusCalculator.Text = _translationService.GetMessage("label.status.route.checker");
            LblLabelStatusMainApp.Text = _translationService.GetMessage("label.status.map.manager");
            BtnOpenViewer.Text = _translationService.GetMessage("button.start.openviewer");
            BtnOpenProfileManager.Text = _translationService.GetMessage("button.start.openprofilemanager");
            LblMultiSport.Text = _translationService.GetMessage("checkbox.start.multisport");

            _statusNotifier.OnStatusNotification += OnStatusNotification;
        }

        private void SetProfileDependencies()
        {
            var hasProfile = !string.IsNullOrEmpty(_profileManager.Profile?.StravaID) ||
                             !string.IsNullOrEmpty(_profileManager.Profile?.RwGpsId);
            ChkMultiSport.IsEnabled = hasProfile;
            ChkMultiSport.IsChecked = hasProfile && _multiSportManager.IsMultiSportDefault;
        }

        private void OnStatusNotification(object sender, OnStatusMessageEventArguments e)
        {
            MainThread.InvokeOnMainThreadAsync(() => UpdateStatus(e.Status));
        }

        private void UpdateStatus(StatusMessage status)
        {
            if (status.Status == StatusMessage.STATUS_PROFILESELECTED)
            {
                SetProfileDependencies();
                return;
            }

            var message = status.Message != null ? _translationService.GetMessage(status.Message) : string.Empty;
            var labelText = $"{status.Status ?? string.Empty} {status.Origin ?? string.Empty} {message}";

            if (status.Origin == StatusMessage.ORG_CALCULATOR)
            {
                LblStatusCalculator.Text = status.Status == StatusMessage.STATUS_START
                    ? status.Message
                    : labelText;
                if (status.Status != StatusMessage.STATUS_START) _calculatorStarted = true;
            }
            else if (status.Origin == StatusMessage.ORG_WEBAPP)
            {
                LblStatusMainApp.Text = status.Status == StatusMessage.STATUS_START
                    ? status.Message
                    : labelText;
                if (status.Status != StatusMessage.STATUS_START) _webappStarted = true;
            }

            if (_calculatorStarted && _webappStarted && !_started)
            {
                BtnOpenViewer.IsEnabled = true;
                _started = true;
            }
        }

        private void OnOpenViewer(object sender, EventArgs e)
        {
            _startViewer = true;

            _multiSportManager.WriteDefaults(ChkMultiSport?.IsChecked ?? false);
            _multiSportManager.RunInMultiSportMode = ChkMultiSport?.IsChecked ?? false;
            _multiSportManager.RunWithSource = !string.IsNullOrEmpty(_profileManager.Profile?.RwGpsId)
                ? "ridewithgps"
                : "strava";

            if (_multiSportManager.RunInMultiSportMode)
            {
                _translationService.MergeMessagesForLanguage("multi");
            }

            var browserPage = _services.GetRequiredService<BrowserPage>();
            Application.Current.Windows[0].Page = new NavigationPage(browserPage);
        }

        private void OnOpenProfileManager(object sender, EventArgs e)
        {
            _profileManagerStarter.Start();
        }

        protected override bool OnBackButtonPressed()
        {
            if (!_startViewer && !_closing)
            {
                _closing = true;
                LblStatusCalculator.Text = _translationService.GetMessage("progress.status.stop");
                LblStatusMainApp.Text = _translationService.GetMessage("progress.status.stop");
                Environment.Exit(0);
            }
            return true;
        }
    }
}
