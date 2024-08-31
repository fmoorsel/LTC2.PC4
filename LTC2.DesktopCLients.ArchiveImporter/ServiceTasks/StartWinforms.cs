using LTC2.DesktopClients.ArchiveImporter.Forms;
using LTC2.DesktopClients.ArchiveImporter.Interfaces;

namespace LTC2.DesktopClients.ArchiveImporter.ServiceTasks
{
    public class StartWinforms : IMainServiceTask
    {
        private readonly ImportForm _profileManagerForm;

        public EventHandler OnReady { get; set; }

        public StartWinforms(ImportForm profileManagerForm)
        {
            _profileManagerForm = profileManagerForm;
        }

        public Task ExecuteAsync()
        {
            Application.Idle += OnIdle;

            Application.Run(_profileManagerForm);

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            return Task.CompletedTask;
        }

        private void OnIdle(object sender, EventArgs e)
        {
            Application.Idle -= OnIdle;

            OnReady?.Invoke(sender, e);
        }

    }
}
