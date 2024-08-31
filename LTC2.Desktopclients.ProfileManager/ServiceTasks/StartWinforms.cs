using LTC2.Desktopclients.ProfileManager.Forms;
using LTC2.Desktopclients.ProfileManager.Interfaces;

namespace LTC2.Desktopclients.ProfileManager.ServiceTasks
{
    public class StartWinforms : IMainServiceTask
    {
        private readonly ProfileManagerForm _profileManagerForm;

        public EventHandler OnReady { get; set; }

        public StartWinforms(ProfileManagerForm profileManagerForm)
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
