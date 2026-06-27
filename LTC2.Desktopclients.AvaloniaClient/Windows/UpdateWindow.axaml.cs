using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LTC2.Desktopclients.AvaloniaClient.Interfaces;
using LTC2.Desktopclients.AvaloniaClient.Services;
using LTC2.Desktopclients.AvaloniaClient.Utils;
using LTC2.Shared.BaseMessages.Interfaces;
using LTC2.Shared.Http.Interfaces;
using LTC2.Shared.Models.Interprocess;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LTC2.Desktopclients.AvaloniaClient.Windows
{
    public partial class UpdateWindow : Window
    {
        private bool _isClosing;
        private bool _isCalculating;
        private bool _isLoaded;
        private bool _intermediatesChecked;
        private DateTime _startUpdate;

        private TextBlock _lblLabelStatusUpdate;
        private TextBlock _lblStatusUpdate;
        private TextBlock _lblLabelProgressUpdate;
        private TextBlock _lblProgressUpdate;

        private RadioButton _rdoNormal;
        private RadioButton _rdoFull;
        private CheckBox _chkReload;
        private Button _btnStartUpdate;
        private Button _btnSelectMultiSports;
        private Border _bdrBottom;

        private readonly ILTC2HttpProxy _ltc2HttpProxy;
        private readonly WebViewConnector _webViewConnector;
        private readonly MultiSportManager _multiSportManager;
        private readonly IBaseTranslationService _translationService;
        private readonly ISelectActivitiesWindowFactory _selectActivitiesWindowFactory;
        private readonly StatusNotifier _statusNotifier;

        public UpdateWindow()
        {
        }

        public UpdateWindow(
            ILTC2HttpProxy ltc2HttpProxy,
            WebViewConnector webViewConnector,
            MultiSportManager multiSportManager,
            IBaseTranslationService translationService,
            ISelectActivitiesWindowFactory selectActivitiesWindowFactory,
            StatusNotifier statusNotifier) : this()
        {
            _ltc2HttpProxy = ltc2HttpProxy;
            _webViewConnector = webViewConnector;
            _multiSportManager = multiSportManager;
            _translationService = translationService;
            _selectActivitiesWindowFactory = selectActivitiesWindowFactory;
            _statusNotifier = statusNotifier;

            InitializeComponent();

            this.Closing += OnClose;
            this.Activated += OnActivated;

            InitControls();

            _statusNotifier.OnStatusNotification += OnStatusNotification;
        }

        private void InitControls()
        {
            _lblLabelStatusUpdate = this.FindControl<TextBlock>("LblLabelStatusUpdate");
            _lblStatusUpdate = this.FindControl<TextBlock>("LblStatusUpdate");
            _lblLabelProgressUpdate = this.FindControl<TextBlock>("LblLabelProgressUpdate");
            _lblProgressUpdate = this.FindControl<TextBlock>("LblProgressUpdate");
            _rdoNormal = this.FindControl<RadioButton>("RdoNormal");
            _rdoFull = this.FindControl<RadioButton>("RdoFull");
            _chkReload = this.FindControl<CheckBox>("ChkReload");
            _btnStartUpdate = this.FindControl<Button>("BtnStartUpdate");
            _btnSelectMultiSports = this.FindControl<Button>("BtnSelectMultiSports");
            _bdrBottom = this.FindControl<Border>("BdrBottom");

            DoTranslate();
        }

        private void DoTranslate()
        {
            TryTranslate(this, "title.update.window");
            TryTranslate(_lblLabelStatusUpdate, "label.running.update");
            TryTranslate(_lblLabelProgressUpdate, "label.progress.update");
            TryTranslate(_lblStatusUpdate, "label.no.update");
            TryTranslate(_lblProgressUpdate, "---");
            TryTranslate(_btnStartUpdate, "button.start.update");
            TryTranslate(_rdoFull, "radio.calc.all.update");
            TryTranslate(_rdoNormal, "radio.normal.update");
            TryTranslate(_chkReload, "checkbox.renew.details");
            TryTranslate(_btnSelectMultiSports, "button.select.multisport");
        }

        private void TryTranslate(Control control, string key)
        {
            var message = _translationService.GetMessage(key);

            switch (control)
            {
                case TextBlock textBlock:
                    textBlock.Text = message;
                    break;
                case RadioButton rb:
                    rb.Content = message;
                    break;
                case CheckBox cb:
                    cb.Content = message;
                    break;
                case Button button:
                    button.Content = message;
                    break;
                case Window window:
                    window.Title = message;
                    break;
            }
        }

        private async void OnActivated(object sender, EventArgs e)
        {
            try
            {
                _isClosing = false;

                if (!_isLoaded)
                {
                    if (_multiSportManager.RunInMultiSportMode)
                    {
                        DoTranslate();

                        _multiSportManager.AthleteId = await _webViewConnector.GetAthleteIdFromTokenAsync();

                        _bdrBottom.Height += 35;
                        Height += 35;

                        _btnSelectMultiSports.IsVisible = true;
                    }

                    _isLoaded = true;
                }

                ShowUpdating();

                await IntermediateCheck();
            }
            catch (Exception)
            {
                // ignore, catch exception to avoid crash
            }
        }

        private void OnClose(object sender, WindowClosingEventArgs args)
        {
            if (!_isClosing)
            {
                args.Cancel = true;
                _isClosing = true;

                Dispatcher.UIThread.Post(SignalStopping);
            }
        }

        private void SignalStopping()
        {
            _isClosing = false;
            Hide();
        }

        private void ShowUpdating(bool limitReached = false, string statusMessage = null)
        {
            _rdoFull.IsEnabled = !_isCalculating;
            _rdoNormal.IsEnabled = !_isCalculating;
            _chkReload.IsEnabled = !_isCalculating && (_rdoFull?.IsChecked ?? false);
            _btnStartUpdate.IsEnabled = !_isCalculating;
            _btnSelectMultiSports.IsEnabled = !_isCalculating;

            if (_isCalculating)
            {
                var startUpdate = $"{_startUpdate}";
                _lblStatusUpdate.Text = _translationService.GetMessage("label.running.update.status", startUpdate);
            }
            else if (limitReached)
            {
                _lblStatusUpdate.Text = _translationService.GetMessage("label.running.update.limit");
                _lblProgressUpdate.Text = _translationService.GetMessage("label.limit.update");
            }
            else
            {
                _lblStatusUpdate.Text = _translationService.GetMessage("label.no.update");
                _lblProgressUpdate.Text = "---";
            }
        }

        private void OnStatusNotification(object sender, OnStatusMessageEventArguments e)
        {
            Dispatcher.UIThread.Post(() => UpdateStatus(e.Status));
        }

        private void UpdateStatus(StatusMessage status)
        {
            if (status.Status == StatusMessage.STATUS_CHECK)
            {
                var msgParts = status.Message.Split(' ');
                var msg = _translationService.GetMessage("label.progress.check.1", msgParts[0]);

                if (msgParts.Length >= 2)
                {
                    msg = _translationService.GetMessage("label.progress.check.2", msgParts.ToList());
                }

                _lblProgressUpdate.Text = msg;
            }
            else if (status.Status == StatusMessage.STATUS_RESULT)
            {
                _isCalculating = false;
                ShowUpdating();
            }
            else if (status.Status == StatusMessage.STATUS_LIMIT)
            {
                _isCalculating = false;
                ShowUpdating(true, status.Message);
            }
            else if (status.Status == StatusMessage.STATUS_WAIT)
            {
                _lblProgressUpdate.Text = _translationService.GetMessage("label.limit.update.quarter", status.Message);
            }
        }

        private async Task IntermediateCheck()
        {
            if (_isCalculating || _intermediatesChecked)
            {
                return;
            }

            _intermediatesChecked = true;

            var token = await _webViewConnector.Login();

            if (string.IsNullOrEmpty(token))
            {
                return;
            }

            var hasIntermediateResult = await _ltc2HttpProxy.HasIntermediateResult(token, _multiSportManager.RunInMultiSportMode);

            if (hasIntermediateResult)
            {
                var text = _translationService.GetMessage("messagebox.intermediate.found");
                var caption = _translationService.GetMessage("messagebox.intermediate.found.header");
                var box = MessageBoxManager.GetMessageBoxStandard(caption, text, ButtonEnum.YesNo);

                var answer = await box.ShowWindowDialogAsync(this);

                if (answer == ButtonResult.Yes)
                {
                    _rdoFull.IsEnabled = false;
                    _rdoNormal.IsEnabled = false;
                    _chkReload.IsEnabled = false;
                    _btnStartUpdate.IsEnabled = false;
                    _btnSelectMultiSports.IsEnabled = false;

                    var textPr = _translationService.GetMessage("messagebox.intermediate.process");
                    var captionPr = _translationService.GetMessage("messagebox.intermediate.process.header");
                    var boxPr = MessageBoxManager.GetMessageBoxStandard(captionPr, textPr, ButtonEnum.Ok);

                    if (_multiSportManager.RunInMultiSportMode)
                    {
                        if (_multiSportManager.RunWithSource == "ridewithgps")
                            await _ltc2HttpProxy.UpdateMulti(token, null, new List<string>(), false, false, true, false, _multiSportManager.RunWithSource);
                        else
                            await _ltc2HttpProxy.UpdateMulti(token, new List<int>(), null, false, false, true, false, _multiSportManager.RunWithSource);
                    }
                    else
                    {
                        await _ltc2HttpProxy.Update(token, false, false, true, false, _multiSportManager.RunWithSource);
                    }

                    await boxPr.ShowWindowDialogAsync(this);
                }
                else
                {
                    if (_multiSportManager.RunInMultiSportMode)
                    {
                        if (_multiSportManager.RunWithSource == "ridewithgps")
                            await _ltc2HttpProxy.UpdateMulti(token, null, new List<string>(), false, false, false, true, _multiSportManager.RunWithSource);
                        else
                            await _ltc2HttpProxy.UpdateMulti(token, new List<int>(), null, false, false, false, true, _multiSportManager.RunWithSource);
                    }
                    else
                    {
                        await _ltc2HttpProxy.Update(token, false, false, false, true, _multiSportManager.RunWithSource);
                    }
                }
            }
        }

        public async void ClickHandlerStartUpdate(object sender, RoutedEventArgs e)
        {
            try
            {
                var token = await _webViewConnector.Login();

                if (token != null)
                {
                    var refresh = _rdoFull?.IsChecked ?? false;
                    var bypassCache = false;

                    if (refresh)
                    {
                        bypassCache = _chkReload?.IsChecked ?? false;
                    }

                    if (_multiSportManager.RunInMultiSportMode)
                    {
                        if (_multiSportManager.RunWithSource == "ridewithgps")
                        {
                            await _ltc2HttpProxy.UpdateMulti(
                                token,
                                new List<int>(),
                                _multiSportManager.CurrentRwGpsActivityTypes,
                                refresh,
                                bypassCache,
                                false,
                                false,
                                _multiSportManager.RunWithSource);
                        }
                        else
                        {
                            var types = _multiSportManager.CurrentActivityTypes.Select(x => (int)x).ToList();

                            await _ltc2HttpProxy.UpdateMulti(
                                token,
                                types,
                                new List<string>(),
                                refresh,
                                bypassCache,
                                false,
                                false,
                                _multiSportManager.RunWithSource);
                        }
                    }
                    else
                    {
                        await _ltc2HttpProxy.Update(token, refresh, bypassCache, false, false, _multiSportManager.RunWithSource);
                    }

                    _isCalculating = true;
                    _startUpdate = DateTime.Now;

                    ShowUpdating();
                }
            }
            catch (Exception)
            {
                // ignore, catch exception to avoid crash
            }
        }

        public async void ClickHandlerSelectMultiSports(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectActivitiesWindow = _selectActivitiesWindowFactory.Create();
                await selectActivitiesWindow.ShowDialog(this);
            }
            catch (Exception)
            {
                // ignore, catch exception to avoid crash
            }
        }

        public void ClickHandlerRdoNormal(object sender, RoutedEventArgs e)
        {
            _chkReload.IsEnabled = false;
            _chkReload.IsChecked = false;
        }

        public void ClickHandlerRdoFull(object sender, RoutedEventArgs e)
        {
            _chkReload.IsEnabled = true;
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            WindowUtilities.RemoveMinimizeButton(this);
        }
    }
}
