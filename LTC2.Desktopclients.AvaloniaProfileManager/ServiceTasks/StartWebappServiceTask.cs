using LTC2.Desktopclients.AvaloniaProfileManager.Models;
using LTC2.Desktopclients.AvaloniaProfileManager.Services;
using LTC2.Shared.Models.Interprocess;
using LTC2.Shared.Utils.Bootstrap.Interfaces;
using LTC2.Shared.Utils.Utils;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace LTC2.Desktopclients.AvaloniaProfileManager.ServiceTasks
{
    public class StartWebappServiceTask : IServiceTask
    {
        private readonly AppSettings _appSettings;
        private readonly StatusNotifier _statusNotifier;

        public StartWebappServiceTask(AppSettings appSettings, StatusNotifier statusNotifier)
        {
            _appSettings = appSettings;
            _statusNotifier = statusNotifier;
        }

        public Task ExecuteAsync()
        {
            _statusNotifier.Initialize();

            _statusNotifier.Notify(new StatusMessage()
            {
                Origin = StatusMessage.ORG_WEBAPP,
                Status = StatusMessage.STATUS_START,
                Message = "Starting..."
            });

            if (_appSettings.WebApp != null)
            {
                var workingDirectory = Path.GetDirectoryName(_appSettings.WebApp);

                var startInfo = new ProcessStartInfo()
                {
                    WorkingDirectory = workingDirectory,
                    FileName = _appSettings.WebApp,
                    CreateNoWindow = _appSettings.WebAppNoWindow
                };

                if (!_appSettings.WebAppNoWindow)
                {
                    startInfo.WindowStyle = _appSettings.WebAppWindowMinimized
                        ? ProcessWindowStyle.Minimized
                        : ProcessWindowStyle.Normal;
                }

                if (_appSettings.WebAppParameters != null)
                {
                    startInfo.Arguments = _appSettings.WebAppParameters;
                }

                var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
                var isMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

                if (isLinux || isMacOS)
                {
                    startInfo.Arguments = $"{_appSettings.WebAppParameters} ppid:{Process.GetCurrentProcess().Id}";
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
