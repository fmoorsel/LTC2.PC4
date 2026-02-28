using LTC2.Desktopclients.WindowsClient.Services;
using LTC2.Shared.Http.Interfaces;
using LTC2.Shared.Messages.Interfaces;
using LTC2.Shared.Models.Interprocess;

namespace LTC2.Desktopclients.WindowsClient.Forms
{
    public partial class UpdateActivitiesForm : Form
    {
        private readonly WebviewConnector _webviewConnector;
        private readonly ILTC2HttpProxy _lTC2HttpProxy;
        private readonly StatusNotifier _statusNotifier;
        private readonly ITranslationService _translationService;
        private readonly MultiSportManager _multiSportManager;
        private readonly SelectActivitiesForm _selectActivitiesForm;
        private readonly SelectRwGpsActivitiesForm _selectRwGpsActivitiesForm;

        private bool _isCalculating;
        private DateTime _startUpdate;
        private bool _formAdaptedToMultiSport;

        private bool _textDone;

        public UpdateActivitiesForm(
            WebviewConnector webviewConnector,
            StatusNotifier statusNotifier,
            ILTC2HttpProxy lTC2HttpProxy,
            MultiSportManager multiSportManager,
            SelectActivitiesForm selectActivitiesForm,
            SelectRwGpsActivitiesForm selectRwGpsActivitiesForm,
            ITranslationService translationService)
        {
            InitializeComponent();

            _webviewConnector = webviewConnector;

            _lTC2HttpProxy = lTC2HttpProxy;
            _statusNotifier = statusNotifier;
            _translationService = translationService;
            _multiSportManager = multiSportManager;
            _selectActivitiesForm = selectActivitiesForm;
            _selectRwGpsActivitiesForm = selectRwGpsActivitiesForm;
        }

        private void rdoRefresh_CheckedChanged(object sender, EventArgs e)
        {
            chkByPassCache.Enabled = rdoRefresh.Checked;

            if (!rdoRefresh.Checked)
            {
                chkByPassCache.Checked = false;
            }
        }

        private async void btnStartUpdate_Click(object sender, EventArgs e)
        {
            var token = await _webviewConnector.Login();

            if (token != null)
            {
                var refresh = rdoRefresh.Checked;
                var bypassCache = false;

                if (refresh)
                {
                    bypassCache = chkByPassCache.Checked;
                }

                if (_multiSportManager.RunInMultiSportMode)
                {
                    if (_multiSportManager.RunWithSource == "ridewithgps")
                    {
                        await _lTC2HttpProxy.UpdateMulti(token, null, _multiSportManager.CurrentRwGpsActivityTypes, refresh, bypassCache, false, false, _multiSportManager.RunWithSource);
                    }
                    else
                    {
                        var types = _multiSportManager.CurrentActivityTypes.Select(x => (int)x).ToList();
                        await _lTC2HttpProxy.UpdateMulti(token, types, null, refresh, bypassCache, false, false, _multiSportManager.RunWithSource);
                    }
                }
                else
                {
                    await _lTC2HttpProxy.Update(token, refresh, bypassCache, false, false, _multiSportManager.RunWithSource);
                }

                _isCalculating = true;
                _startUpdate = DateTime.Now;

                ShowUpdating();
            }
        }

        private void ShowUpdating(bool limitReached = false, string statusMessage = null)
        {
            grpStartUpdate.Enabled = !_isCalculating;

            if (_isCalculating)
            {
                var startUpdate = $"{_startUpdate}";
                lblStatusRunningUpdates.Text = _translationService.GetMessage("label.running.update.status", startUpdate);
            }
            else if (limitReached)
            {
                if (statusMessage == null)
                {
                    statusMessage = DateTime.Now.AddDays(1).ToString();
                }

                lblStatusRunningUpdates.Text = _translationService.GetMessage("label.running.update.limit");
                lblProgress.Text = _translationService.GetMessage("label.limit.update");
            }
            else
            {
                lblStatusRunningUpdates.Text = _translationService.GetMessage("label.no.update");
                lblProgress.Text = "---";
            }
        }

        private void UpdateActivitiesForm_VisibleChanged(object sender, EventArgs e)
        {
            ShowUpdating();
        }

        private async void UpdateActivitiesForm_Load(object sender, EventArgs e)
        {
            _statusNotifier.OnStatusNotification += OnStatusNotification;

            if (!_textDone)
            {
                _translationService.LoadMessagesForForm(this);
                _textDone = true;
            }

            if (!_isCalculating)
            {
                chkByPassCache.Checked = false;
                rdoRefresh.Checked = false;
                rdoUpdate.Checked = true;

                var token = await _webviewConnector.Login();

                if (token != null)
                {
                    _multiSportManager.AthleteId = await _webviewConnector.GetAthleteIdFromToken();

                    var hasIntermediateResult = await _lTC2HttpProxy.HasIntermediateResult(token, _multiSportManager.RunInMultiSportMode);

                    if (hasIntermediateResult)
                    {
                        var antwoord = MessageBox.Show(_translationService.GetMessage("messagebox.intermediate.found"), _translationService.GetMessage("messagebox.intermediate.found.header"), MessageBoxButtons.YesNo);

                        if (antwoord == DialogResult.Yes)
                        {
                            MessageBox.Show(_translationService.GetMessage("messagebox.intermediate.process"), _translationService.GetMessage("messagebox.intermediate.process.header"), MessageBoxButtons.OK);

                            if (_multiSportManager.RunInMultiSportMode)
                            {
                                if (_multiSportManager.RunWithSource == "ridewithgps")
                                    await _lTC2HttpProxy.UpdateMulti(token, null, new List<string>(), false, false, true, false, _multiSportManager.RunWithSource);
                                else
                                    await _lTC2HttpProxy.UpdateMulti(token, new List<int>(), null, false, false, true, false, _multiSportManager.RunWithSource);
                            }
                            else
                            {
                                await _lTC2HttpProxy.Update(token, false, false, true, false, _multiSportManager.RunWithSource);
                            }
                        }
                        else
                        {
                            if (_multiSportManager.RunInMultiSportMode)
                            {
                                if (_multiSportManager.RunWithSource == "ridewithgps")
                                    await _lTC2HttpProxy.UpdateMulti(token, null, new List<string>(), false, false, false, true, _multiSportManager.RunWithSource);
                                else
                                    await _lTC2HttpProxy.UpdateMulti(token, new List<int>(), null, false, false, false, true, _multiSportManager.RunWithSource);
                            }
                            else
                            {
                                await _lTC2HttpProxy.Update(token, false, false, false, true, _multiSportManager.RunWithSource);
                            }
                        }
                    }
                }

                if (_multiSportManager.RunInMultiSportMode)
                {
                    AdaptFormToMultiSport();

                    btnSelectSports.Enabled = true;
                }
            }

        }

        public void AdaptFormToMultiSport()
        {
            if (!_formAdaptedToMultiSport && _multiSportManager.RunInMultiSportMode)
            {
                btnSelectSports.Visible = true;
                var _userDefaultScreenDPI = 96.0f;
                using (var g = CreateGraphics())
                {
                    var yDPI = g.DpiY;
                    var dpiScale = yDPI / _userDefaultScreenDPI;
                    Height += Convert.ToInt32(30.0 * dpiScale);
                    grpStartUpdate.Height += Convert.ToInt32(35.0 * dpiScale);
                }
                
                _translationService.LoadMessagesForForm(this);
                
                _formAdaptedToMultiSport = true;
            }
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
            if (status.Status == StatusMessage.STATUS_CHECK)
            {
                var msgParts = status.Message.Split(' ');
                var msg = _translationService.GetMessage("label.progress.check.1", msgParts[0]);

                if (msgParts.Length >= 2)
                {
                    msg = _translationService.GetMessage("label.progress.check.2", msgParts.ToList());
                }

                lblProgress.Text = msg;
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
                lblProgress.Text = _translationService.GetMessage("label.limit.update.quarter", status.Message);
            }
        }

        private void UpdateActivitiesForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _statusNotifier.OnStatusNotification -= OnStatusNotification;
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

        private async void btnSelectSports_Click(object sender, EventArgs e)
        {
            _multiSportManager.AthleteId = await _webviewConnector.GetAthleteIdFromToken();

            if (_multiSportManager.RunWithSource == "ridewithgps")
                _selectRwGpsActivitiesForm.ShowDialog();
            else
                _selectActivitiesForm.ShowDialog();
        }
    }
}
