using LTC2.Desktopclients.MauiClient.Interfaces;
using LTC2.Desktopclients.MauiClient.Services;
using LTC2.Shared.BaseMessages.Interfaces;
using LTC2.Shared.Http.Interfaces;
using LTC2.Shared.Models.Interprocess;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LTC2.Desktopclients.MauiClient.Pages
{
    public partial class UpdatePage : ContentPage
    {
        private bool _isCalculating;
        private bool _isLoaded;
        private bool _intermediatesChecked;
        private DateTime _startUpdate;

        private readonly ILTC2HttpProxy _ltc2HttpProxy;
        private readonly WebViewConnector _webViewConnector;
        private readonly MultiSportManager _multiSportManager;
        private readonly IBaseTranslationService _translationService;
        private readonly ISelectActivitiesPageFactory _selectActivitiesPageFactory;
        private readonly StatusNotifier _statusNotifier;

        public UpdatePage(
            ILTC2HttpProxy ltc2HttpProxy,
            WebViewConnector webViewConnector,
            MultiSportManager multiSportManager,
            IBaseTranslationService translationService,
            ISelectActivitiesPageFactory selectActivitiesPageFactory,
            StatusNotifier statusNotifier)
        {
            _ltc2HttpProxy = ltc2HttpProxy;
            _webViewConnector = webViewConnector;
            _multiSportManager = multiSportManager;
            _translationService = translationService;
            _selectActivitiesPageFactory = selectActivitiesPageFactory;
            _statusNotifier = statusNotifier;

            InitializeComponent();
            DoTranslate();

            _statusNotifier.OnStatusNotification += OnStatusNotification;
        }

        private void DoTranslate()
        {
            Title = _translationService.GetMessage("title.update.window");
            LblLabelStatusUpdate.Text = _translationService.GetMessage("label.running.update");
            LblStatusUpdate.Text = _translationService.GetMessage("label.no.update");
            LblLabelProgressUpdate.Text = _translationService.GetMessage("label.progress.update");
            LblProgressUpdate.Text = "---";
            BtnStartUpdate.Text = _translationService.GetMessage("button.start.update");
            LblRdoNormal.Text = _translationService.GetMessage("radio.normal.update");
            LblRdoFull.Text = _translationService.GetMessage("radio.calc.all.update");
            LblChkReload.Text = _translationService.GetMessage("checkbox.renew.details");
            BtnSelectMultiSports.Text = _translationService.GetMessage("button.select.multisport");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                if (!_isLoaded)
                {
                    if (_multiSportManager.RunInMultiSportMode)
                    {
                        DoTranslate();
                        _multiSportManager.AthleteId = await _webViewConnector.GetAthleteIdFromTokenAsync();
                        BtnSelectMultiSports.IsVisible = true;
                    }
                    _isLoaded = true;
                }

                ShowUpdating();
                await IntermediateCheck();
            }
            catch (Exception) { }
        }

        private void ShowUpdating(bool limitReached = false)
        {
            RdoFull.IsEnabled = !_isCalculating;
            RdoNormal.IsEnabled = !_isCalculating;
            ChkReload.IsEnabled = !_isCalculating && (RdoFull?.IsChecked ?? false);
            BtnStartUpdate.IsEnabled = !_isCalculating;
            BtnSelectMultiSports.IsEnabled = !_isCalculating;

            if (_isCalculating)
            {
                LblStatusUpdate.Text = _translationService.GetMessage("label.running.update.status", $"{_startUpdate}");
            }
            else if (limitReached)
            {
                LblStatusUpdate.Text = _translationService.GetMessage("label.running.update.limit");
                LblProgressUpdate.Text = _translationService.GetMessage("label.limit.update");
            }
            else
            {
                LblStatusUpdate.Text = _translationService.GetMessage("label.no.update");
                LblProgressUpdate.Text = "---";
            }
        }

        private void OnStatusNotification(object sender, OnStatusMessageEventArguments e)
        {
            MainThread.InvokeOnMainThreadAsync(() => UpdateStatus(e.Status));
        }

        private void UpdateStatus(StatusMessage status)
        {
            if (status.Status == StatusMessage.STATUS_CHECK)
            {
                var msgParts = status.Message.Split(' ');
                var msg = _translationService.GetMessage("label.progress.check.1", msgParts[0]);
                if (msgParts.Length >= 2) msg = _translationService.GetMessage("label.progress.check.2", msgParts.ToList());
                LblProgressUpdate.Text = msg;
            }
            else if (status.Status == StatusMessage.STATUS_RESULT)
            {
                _isCalculating = false;
                ShowUpdating();
            }
            else if (status.Status == StatusMessage.STATUS_LIMIT)
            {
                _isCalculating = false;
                ShowUpdating(true);
            }
            else if (status.Status == StatusMessage.STATUS_WAIT)
            {
                LblProgressUpdate.Text = _translationService.GetMessage("label.limit.update.quarter", status.Message);
            }
        }

        private async Task IntermediateCheck()
        {
            if (_isCalculating || _intermediatesChecked) return;
            _intermediatesChecked = true;

            var token = await _webViewConnector.Login();
            if (string.IsNullOrEmpty(token)) return;

            var hasIntermediateResult = await _ltc2HttpProxy.HasIntermediateResult(token, _multiSportManager.RunInMultiSportMode);
            if (!hasIntermediateResult) return;

            var text = _translationService.GetMessage("messagebox.intermediate.found");
            var caption = _translationService.GetMessage("messagebox.intermediate.found.header");
            var answer = await DisplayAlert(caption, text, "Ja", "Nee");

            if (answer)
            {
                SetControlsEnabled(false);

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

                var textPr = _translationService.GetMessage("messagebox.intermediate.process");
                var captionPr = _translationService.GetMessage("messagebox.intermediate.process.header");
                await DisplayAlert(captionPr, textPr, "OK");
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

        private void SetControlsEnabled(bool enabled)
        {
            RdoFull.IsEnabled = enabled;
            RdoNormal.IsEnabled = enabled;
            ChkReload.IsEnabled = enabled;
            BtnStartUpdate.IsEnabled = enabled;
            BtnSelectMultiSports.IsEnabled = enabled;
        }

        private async void OnStartUpdate(object sender, EventArgs e)
        {
            try
            {
                var token = await _webViewConnector.Login();
                if (token == null) return;

                var refresh = RdoFull?.IsChecked ?? false;
                var bypassCache = refresh && (ChkReload?.IsChecked ?? false);

                if (_multiSportManager.RunInMultiSportMode)
                {
                    if (_multiSportManager.RunWithSource == "ridewithgps")
                        await _ltc2HttpProxy.UpdateMulti(token, new List<int>(), _multiSportManager.CurrentRwGpsActivityTypes, refresh, bypassCache, false, false, _multiSportManager.RunWithSource);
                    else
                        await _ltc2HttpProxy.UpdateMulti(token, _multiSportManager.CurrentActivityTypes.Select(x => (int)x).ToList(), new List<string>(), refresh, bypassCache, false, false, _multiSportManager.RunWithSource);
                }
                else
                {
                    await _ltc2HttpProxy.Update(token, refresh, bypassCache, false, false, _multiSportManager.RunWithSource);
                }

                _isCalculating = true;
                _startUpdate = DateTime.Now;
                ShowUpdating();
            }
            catch (Exception) { }
        }

        private async void OnSelectMultiSports(object sender, EventArgs e)
        {
            try
            {
                var page = _selectActivitiesPageFactory.Create();
                await Navigation.PushModalAsync(page);
                await page.WaitForClose();
            }
            catch (Exception) { }
        }

        private void OnRdoNormal(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                ChkReload.IsEnabled = false;
                ChkReload.IsChecked = false;
                PnlReload.IsVisible = false;
            }
        }

        private void OnRdoFull(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                ChkReload.IsEnabled = true;
                PnlReload.IsVisible = true;
            }
        }

        private async void OnClose(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
