using LTC2.Desktopclients.WindowsClient.Forms;
using LTC2.Desktopclients.WindowsClient.Interfaces;

namespace LTC2.Desktopclients.WindowsClient.ServiceTasks
{
    public class StartWinforms : IMainServiceTask
    {
        private readonly SplashScreen _splashscreen;
        private readonly MainForm _mainForm;

        public EventHandler OnReady { get; set; }

        public StartWinforms(SplashScreen splashscreen, MainForm mainForm)
        {
            _splashscreen = splashscreen;
            _mainForm = mainForm;
        }

        public Task ExecuteAsync()
        {
            Application.Idle += OnIdle;

            Application.Run(_mainForm);

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
