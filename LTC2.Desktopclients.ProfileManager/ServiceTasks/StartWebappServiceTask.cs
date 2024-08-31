using LTC2.Desktopclients.ProfileManager.Models;
using LTC2.Shared.Utils.Bootstrap.Interfaces;
using LTC2.Shared.Utils.Utils;
using System.Diagnostics;

namespace LTC2.Desktopclients.ProfileManager.ServiceTasks
{
    public class StartWebappServiceTask : IServiceTask
    {
        private readonly AppSettings _appSettings;

        public StartWebappServiceTask(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public Task ExecuteAsync()
        {
            if (_appSettings.WebApp != null)
            {
                var workingDirectory = Path.GetDirectoryName(_appSettings.WebApp);

                var startInfo = new ProcessStartInfo();

                startInfo.WorkingDirectory = workingDirectory;
                startInfo.FileName = _appSettings.WebApp;
                startInfo.CreateNoWindow = _appSettings.WebAppNoWindow;

                if (!_appSettings.WebAppNoWindow)
                {
                    startInfo.WindowStyle = _appSettings.WebAppWindowMinimized ? ProcessWindowStyle.Minimized : ProcessWindowStyle.Normal;
                }

                if (_appSettings.WebAppParameters != null)
                {
                    startInfo.Arguments = _appSettings.WebAppParameters;
                }

                var process = Process.Start(startInfo);

                ChildProcessTracker.AddProcess(process);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            return Task.CompletedTask;
        }
    }
}
