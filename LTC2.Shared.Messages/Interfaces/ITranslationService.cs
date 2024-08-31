using LTC2.Shared.BaseMessages.Interfaces;
using System.Windows.Forms;

namespace LTC2.Shared.Messages.Interfaces
{
    public interface ITranslationService : IBaseTranslationService
    {
        public void LoadMessagesForForm(Form form);

        public void LoadMessagesForControl(Control control);
    }
}
