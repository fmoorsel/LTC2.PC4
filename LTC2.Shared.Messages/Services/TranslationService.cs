using LTC2.Shared.BaseMessages.Services;
using LTC2.Shared.Messages.Interfaces;
using System;
using System.Windows.Forms;

namespace LTC2.Shared.Messages.Services
{
    public class TranslationService : BaseTranslationService, ITranslationService
    {
        public void LoadMessagesForForm(Form form)
        {
            form.Text = GetMessage(form.Text);

            foreach (var control in form.Controls)
            {
                if (control is Control)
                {
                    LoadMessagesForControl(control as Control);
                }
            }
        }

        public void LoadMessagesForControl(Control control)
        {
            if (control.Text.StartsWith('#') && control.Text.Length > 1)
            {
                var id = control.Text;

                control.Text = GetMessage(id);
            }

            if (control is ContainerControl)
            {
                foreach (var embeddedControl in control.Controls)
                {
                    if (embeddedControl is Control)
                    {
                        LoadMessagesForControl(embeddedControl as Control);
                    }
                }
            }
            else
            {
                try
                {
                    var property = control.GetType().GetProperty("Controls");
                    var container = property.GetValue(control);

                    if (container != null && container is Control.ControlCollection)
                    {
                        var containerControl = container as Control.ControlCollection;

                        foreach (var embeddedControl in containerControl)
                        {
                            if (embeddedControl is Control)
                            {
                                LoadMessagesForControl(embeddedControl as Control);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
