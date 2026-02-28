using LTC2.Desktopclients.WindowsClient.Services;
using LTC2.Shared.Messages.Interfaces;
using LTC2.Shared.Models.Domain;

namespace LTC2.Desktopclients.WindowsClient.Forms
{
    public partial class SelectActivitiesForm : Form
    {
        private readonly MultiSportManager _multiSportManager;
        private readonly ITranslationService _translationService;

        public SelectActivitiesForm(
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

        private void SelectActivitiesForm_Load(object sender, EventArgs e)
        {
            chlSelectActivities.Items.Clear();

            var athleteActivities = _multiSportManager.CurrentActivityTypes;

            _multiSportManager.GetActivityTypes().ForEach(activity =>
            {
                chlSelectActivities.Items.Add(activity.Description, athleteActivities.Contains(activity.ActivityType));
            });
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var currentActivityTypes = new List<GenericActivityType>();

            _multiSportManager.GetActivityTypes().ForEach(activity =>
            {
                if (chlSelectActivities.CheckedItems.Contains(activity.Description))
                {
                    currentActivityTypes.Add(activity.ActivityType);
                }
            });

            _multiSportManager.CurrentActivityTypes = currentActivityTypes;

            Close();
        }
    }
}
