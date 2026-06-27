using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LTC2.Desktopclients.AvaloniaClient.Services;
using LTC2.Desktopclients.AvaloniaClient.Utils;
using LTC2.Shared.BaseMessages.Interfaces;
using LTC2.Shared.Models.Interprocess;
using System;

namespace LTC2.Desktopclients.AvaloniaClient.Windows
{
    public partial class StartWindow : Window
    {
        private readonly BrowserWindow _browserWindow;
        private readonly ApplicationManager _applicationManager;
        private readonly IBaseTranslationService _translationService;
        private readonly ProfileManagerStarter _profileManagerStarter;
        private readonly MultiSportManager _multiSportManager;
        private readonly ProfileManager _profileManager;

        private TextBlock _lblLabelStatusCalculator;
        private TextBlock _lblLabelStatusMainApp;
        private TextBlock _lblStatusCalculator;
        private TextBlock _lblStatusMainApp;
        private Button _btnOpenViewer;
        private Button _btnOpenProfileManager;
        private CheckBox _chkMultiSport;

        private bool _started;
        private bool _startViewer;
        private bool _calculatorStarted;
        private bool _webappStarted;
        private bool _closing;

        public StartWindow()
        {
        }

        public StartWindow(
            ApplicationManager applicationManager,
            BrowserWindow browserWindow,
            StatusNotifier statusNotifier,
            ProfileManagerStarter profileManagerStarter,
            ProfileManager profileManager,
            MultiSportManager multiSportManager,
            IBaseTranslationService translationService) : this()
        {
            _browserWindow = browserWindow;
            _applicationManager = applicationManager;
            _translationService = translationService;
            _profileManagerStarter = profileManagerStarter;
            _multiSportManager = multiSportManager;
            _profileManager = profileManager;

            statusNotifier.OnStatusNotification += OnStatusNotification;

            InitializeComponent();
            InitControls();
        }

        private void InitControls()
        {
            this.Closing += OnClose;

            _lblLabelStatusCalculator = this.FindControl<TextBlock>("LblLabelStatusCalculator");
            _lblLabelStatusMainApp = this.FindControl<TextBlock>("LblLabelStatusMainApp");
            _lblStatusCalculator = this.FindControl<TextBlock>("LblStatusCalculator");
            _lblStatusMainApp = this.FindControl<TextBlock>("LblStatusMainApp");
            _btnOpenViewer = this.FindControl<Button>("BtnOpenViewer");
            _btnOpenProfileManager = this.FindControl<Button>("BtnOpenProfileManager");
            _chkMultiSport = this.FindControl<CheckBox>("ChkMultiSport");

            TryTranslate(this, "title.start.window");
            TryTranslate(_lblLabelStatusCalculator, _translationService.GetMessage("label.status.route.checker"));
            TryTranslate(_lblLabelStatusMainApp, _translationService.GetMessage("label.status.map.manager"));
            TryTranslate(_btnOpenViewer, _translationService.GetMessage("button.start.openviewer"));
            TryTranslate(_btnOpenProfileManager, _translationService.GetMessage("button.start.openprofilemanager"));
            TryTranslate(_chkMultiSport, _translationService.GetMessage("checkbox.start.multisport"));

        }

        private void SetProfileDependencies()
        {
            if (_chkMultiSport == null) return;
            var hasProfile = !string.IsNullOrEmpty(_profileManager.Profile?.StravaID) || !string.IsNullOrEmpty(_profileManager.Profile?.RwGpsId);
            _chkMultiSport.IsEnabled = hasProfile;
            _chkMultiSport.IsChecked = hasProfile && _multiSportManager.IsMultiSportDefault;
        }

        private void TryTranslate(Control control, string message)
        {
            if (control is TextBlock textBlock)
            {
                textBlock.Text = _translationService.GetMessage(message);
            }

            if (control is Button button)
            {
                button.Content = _translationService.GetMessage(message);
            }

            if (control is CheckBox checkBox)
            {
                checkBox.Content = _translationService.GetMessage(message);
            }

            if (control is Window window)
            {
                window.Title = _translationService.GetMessage(message);
            }
        }

        private void OnStatusNotification(object sender, OnStatusMessageEventArguments e)
        {
            Dispatcher.UIThread.Post(() => UpdateStatus(e.Status));
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
                if (status.Status == StatusMessage.STATUS_START)
                {
                    _lblStatusCalculator.Text = status.Message;
                }
                else
                {
                    _lblStatusCalculator.Text = labelText;
                    _calculatorStarted = true;
                }
            }
            else if (status.Origin == StatusMessage.ORG_WEBAPP)
            {
                if (status.Status == StatusMessage.STATUS_START)
                {
                    _lblStatusMainApp.Text = status.Message;
                }
                else
                {
                    _lblStatusMainApp.Text = labelText;
                    _webappStarted = true;
                }
            }

            if (_calculatorStarted && _webappStarted && !_started)
            {
                _btnOpenViewer.IsEnabled = true;
                _btnOpenViewer.Focusable = true;
                _btnOpenViewer.Focus();
                _started = true;
            }
        }

        public void ClickHandlerStartViewer(object sender, RoutedEventArgs args)
        {
            _startViewer = true;

            _multiSportManager.WriteDefaults(_chkMultiSport?.IsChecked ?? false);
            _multiSportManager.RunInMultiSportMode = _chkMultiSport?.IsChecked ?? false;
            _multiSportManager.RunWithSource = !string.IsNullOrEmpty(_profileManager.Profile.RwGpsId) ? "ridewithgps" : "strava";

            if (_multiSportManager.RunInMultiSportMode)
            {
                _translationService.MergeMessagesForLanguage("multi");
            }

            _applicationManager.MainWindow = _browserWindow;

            _browserWindow.Show();

            Close();
        }

        public void ClickHandlerStartProfileManager(object sender, RoutedEventArgs args)
        {
            _profileManagerStarter.Start();
        }

        private void SignalStopping()
        {
            var stopMessage = _translationService.GetMessage("progress.status.stop");

            _lblStatusCalculator.Text = stopMessage;
            _lblStatusMainApp.Text = stopMessage;

            Environment.Exit(0);
        }

        public void OnClose(object sender, WindowClosingEventArgs args)
        {
            if (!_startViewer && !_closing)
            {
                args.Cancel = true;
                _closing = true;

                Dispatcher.UIThread.Post(SignalStopping);
            }
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            WindowUtilities.RemoveMinimizeButton(this);
        }
    }
}
