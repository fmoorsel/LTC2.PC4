using LTC2.Desktopclients.WindowsClient.Interfaces;
using LTC2.Desktopclients.WindowsClient.Models;
using System.Diagnostics;

namespace LTC2.Desktopclients.WindowsClient.ServiceTasks
{
    public class CheckTilesPrerequisiteTask : IPrerequisiteServiceTask, IInterruptable
    {
        public bool ShouldStop { get; set; }

        private readonly AppSettings _appSettings;

        public CheckTilesPrerequisiteTask(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public Task ExecuteAsync()
        {
            if (!File.Exists(_appSettings.TilesOKFile))
            {
                MessageBox.Show("De kaart-bestanden worden geinstalleerd en dit kan even duren.", "Nog een post-installatie stap...", MessageBoxButtons.OK);
                var workingDirectory = Path.GetDirectoryName(_appSettings.CustomInstallAction);

                var startInfo = new ProcessStartInfo();

                startInfo.WorkingDirectory = workingDirectory;
                startInfo.FileName = _appSettings.CustomInstallAction;
                startInfo.WindowStyle = ProcessWindowStyle.Normal;
                startInfo.Arguments = _appSettings.CustomInstallActionParameters;

                var process = Process.Start(startInfo);

                ShouldStop = true;
            }

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            return Task.CompletedTask;
        }
    }
}
