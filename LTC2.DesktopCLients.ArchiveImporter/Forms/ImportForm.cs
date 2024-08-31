using LTC2.DesktopClients.ArchiveImporter.Services;
using LTC2.Shared.Messages.Interfaces;

namespace LTC2.DesktopClients.ArchiveImporter.Forms
{
    public partial class ImportForm : Form
    {

        private readonly OpenFileDialog _openFileDialog;
        private readonly ArchiveProcessor _archiveProcessor;
        private readonly ITranslationService _translationService;

        private bool _processing = false;

        public ImportForm(ArchiveProcessor archiveProcessor, ITranslationService translationService)
        {
            InitializeComponent();

            this.AutoScaleMode = AutoScaleMode.Dpi;

            _openFileDialog = new OpenFileDialog();
            _openFileDialog.Filter = "Zip files (*.zip)|*.zip|All files (*.*)|*.*";

            _archiveProcessor = archiveProcessor;
            _translationService = translationService;

            _translationService.LoadMessagesForForm(this);
        }

        private void StatusUpdater(string status)
        {
            if (InvokeRequired)
            {
                var updater = new ArchiveImportUpdateStatusDelegate(UpdateStatus);

                BeginInvoke(updater, status);
            }
            else
            {
                UpdateStatus(status);
            }
        }

        private void UpdateStatus(string status)
        {
            var builderMessage = BuildMessage(status);
            lblStatusImport.Text = builderMessage;
        }

        private string BuildMessage(string status)
        {
            var parts = status.Split(' ');
            var messageparts = parts.Select(p =>
            {
                if (p.StartsWith("#"))
                {
                    var translation = _translationService.GetMessage(p);

                    return translation;
                }

                return p;
            }).ToList();

            return string.Join(" ", messageparts);
        }

        private void btnChooseFile_Click(object sender, EventArgs e)
        {
            var dialoresult = _openFileDialog.ShowDialog();

            if (dialoresult == DialogResult.OK && File.Exists(_openFileDialog.FileName))
            {
                txtFile.Text = _openFileDialog.FileName;

                lblStatusImport.Text = txtFile.Text;

                btnStartImport.Enabled = File.Exists(_openFileDialog.FileName);
            }

        }

        private async void btnImport_Click(object sender, EventArgs e)
        {
            var file = _openFileDialog.FileName;

            _processing = true;

            btnChooseFile.Enabled = false;
            btnStartImport.Enabled = false;
            btnRemoveArchive.Enabled = false;

            try
            {
                var success = await _archiveProcessor.Process(file, StatusUpdater);


                if (success)
                {
                    var header = _translationService.GetMessage("#import.success.header");
                    var messageFailed = _translationService.GetMessage("#import.success");

                    MessageBox.Show(messageFailed, header);
                }
                else
                {
                    var header = _translationService.GetMessage("#import.failed.header");
                    var messageFailed = _translationService.GetMessage("#import.failed");

                    MessageBox.Show(messageFailed, header);

                    lblStatusImport.Text = string.Empty;
                }
            }
            catch (Exception)
            {
                var header = _translationService.GetMessage("#import.failed.header");
                var messageFailed = _translationService.GetMessage("#import.failed");

                MessageBox.Show(messageFailed, header);

                lblStatusImport.Text = string.Empty;
            }
            finally
            {

                btnChooseFile.Enabled = true;
                btnStartImport.Enabled = File.Exists(_openFileDialog.FileName);

                UpdateArchiveList();

                _processing = false;
            }
        }

        private void ImportForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_processing)
            {
                var header = _translationService.GetMessage("#message.processing.header");
                var question = _translationService.GetMessage("#message.processing");

                var answer = MessageBox.Show(question, header, MessageBoxButtons.YesNo);

                e.Cancel = (answer == DialogResult.No);

            }
        }

        private void ImportForm_Load(object sender, EventArgs e)
        {
            UpdateArchiveList();
        }

        private void UpdateArchiveList()
        {
            var archives = _archiveProcessor.GetArchives();

            lstArchives.Items.Clear();
            foreach (var archive in archives)
            {
                lstArchives.Items.Add(archive);
            }

            if (lstArchives.Items.Count > 0)
            {
                lstArchives.SelectedIndex = 0;
                btnRemoveArchive.Enabled = true;
            }
            else
            {
                btnRemoveArchive.Enabled = false;
            }
        }

        private void btnRemoveArchive_Click(object sender, EventArgs e)
        {
            if (lstArchives.SelectedIndex >= 0)
            {
                var archive = lstArchives.SelectedItem as string;

                _archiveProcessor.RemoveArchive(archive);

                UpdateArchiveList();
            }
        }
    }
}
