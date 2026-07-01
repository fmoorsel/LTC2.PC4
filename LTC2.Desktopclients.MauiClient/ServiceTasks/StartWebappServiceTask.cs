using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using LTC2.Desktopclients.MauiClient.Models;
using LTC2.Desktopclients.MauiClient.Services;
using LTC2.Shared.BaseMessages.Interfaces;
using LTC2.Shared.Models.Interprocess;
using LTC2.Shared.Utils.Bootstrap.Interfaces;
using LTC2.Shared.Utils.Utils;

namespace LTC2.Desktopclients.MauiClient.ServiceTasks
{
    public class StartWebappServiceTask : IServiceTask
    {
        private readonly AppSettings _appSettings;
        private readonly ProfileManager _profileManager;
        private readonly StatusNotifier _statusNotifier;
        private readonly IBaseTranslationService _translationService;

        public StartWebappServiceTask(
            AppSettings appSettings,
            ProfileManager profileManager,
            StatusNotifier statusNotifier,
            IBaseTranslationService translationService)
        {
            _appSettings = appSettings;
            _profileManager = profileManager;
            _statusNotifier = statusNotifier;
            _translationService = translationService;
        }

        public Task ExecuteAsync()
        {
            SendStartStatus();

            if (_appSettings.WebApp != null)
            {
                var startInfo = new ProcessStartInfo();
                startInfo.WorkingDirectory = Path.GetDirectoryName(_appSettings.WebApp);
                startInfo.FileName = _appSettings.WebApp;
                startInfo.CreateNoWindow = _appSettings.WebAppNoWindow;

                if (!_appSettings.WebAppNoWindow)
                    startInfo.WindowStyle = _appSettings.WebAppWindowMinimized ? ProcessWindowStyle.Minimized : ProcessWindowStyle.Normal;

                if (_appSettings.WebAppParameters != null)
                    startInfo.Arguments = $"{_appSettings.WebAppParameters} prof:{_profileManager.Profile.ID}";

                var process = Process.Start(startInfo);
                ChildProcessTracker.AddProcess(process);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync() => Task.CompletedTask;

        private void SendStartStatus()
        {
            _statusNotifier.Initialize();
            _statusNotifier.Notify(new StatusMessage()
            {
                Origin = StatusMessage.ORG_WEBAPP,
                Status = StatusMessage.STATUS_START,
                Message = _translationService.GetMessage("progress.status.start")
            });
        }
    }
}
