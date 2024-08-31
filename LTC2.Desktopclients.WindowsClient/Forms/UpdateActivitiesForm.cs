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

        private bool _isCalculating;
        private DateTime _startUpdate;

        public UpdateActivitiesForm(
            WebviewConnector webviewConnector,
            StatusNotifier statusNotifier,
            ILTC2HttpProxy lTC2HttpProxy,
            ITranslationService translationService)
        {
            InitializeComponent();

            _webviewConnector = webviewConnector;

            _lTC2HttpProxy = lTC2HttpProxy;
            _statusNotifier = statusNotifier;
            _translationService = translationService;

            _translationService.LoadMessagesForForm(this);
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

                await _lTC2HttpProxy.Update(token, refresh, bypassCache, false, false);

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

            if (!_isCalculating)
            {
                chkByPassCache.Checked = false;
                rdoRefresh.Checked = false;
                rdoUpdate.Checked = true;

                var token = await _webviewConnector.Login();

                if (token != null)
                {
                    var hasIntermediateResult = await _lTC2HttpProxy.HasIntermediateResult(token);

                    if (hasIntermediateResult)
                    {
                        var antwoord = MessageBox.Show(_translationService.GetMessage("messagebox.intermediate.found"), _translationService.GetMessage("messagebox.intermediate.found.header"), MessageBoxButtons.YesNo);

                        if (antwoord == DialogResult.Yes)
                        {
                            MessageBox.Show(_translationService.GetMessage("messagebox.intermediate.process"), _translationService.GetMessage("messagebox.intermediate.process.header"), MessageBoxButtons.OK);

                            await _lTC2HttpProxy.Update(token, false, false, true, false);
                        }
                        else
                        {
                            await _lTC2HttpProxy.Update(token, false, false, false, true);
                        }
                    }
                }
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
    }
}
