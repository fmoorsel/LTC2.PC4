using LTC2.Desktopclients.WindowsClient.Services;
using LTC2.Shared.Messages.Interfaces;

namespace LTC2.Desktopclients.WindowsClient.Forms
{
    public partial class SelectRwGpsActivitiesForm : Form
    {
        private readonly MultiSportManager _multiSportManager;
        private readonly ITranslationService _translationService;

        public SelectRwGpsActivitiesForm(
            MultiSportManager multiSportManager,
            ITranslationService translationService)
        {
            InitializeComponent();

            _multiSportManager = multiSportManager;
            _translationService = translationService;

            _translationService.LoadMessagesForForm(this);
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

        private void SelectRwGpsActivitiesForm_Load(object sender, EventArgs e)
        {
            chlSelectActivities.Items.Clear();

            var athleteActivities = _multiSportManager.CurrentRwGpsActivityTypes;

            _multiSportManager.GetRwGpsActivityTypes().ForEach(activity =>
            {
                chlSelectActivities.Items.Add(activity.Description, athleteActivities.Contains(activity.Value));
            });
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var currentActivityTypes = new List<string>();

            _multiSportManager.GetRwGpsActivityTypes().ForEach(activity =>
            {
                if (chlSelectActivities.CheckedItems.Contains(activity.Description))
                {
                    currentActivityTypes.Add(activity.Value);
                }
            });

            _multiSportManager.CurrentRwGpsActivityTypes = currentActivityTypes;

            Close();
        }
    }
}
