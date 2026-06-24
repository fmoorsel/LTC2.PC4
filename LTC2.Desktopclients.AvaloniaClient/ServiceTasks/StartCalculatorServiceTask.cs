using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using LTC2.Desktopclients.AvaloniaClient.Models;
using LTC2.Desktopclients.AvaloniaClient.Services;
using LTC2.Shared.BaseMessages.Interfaces;
using LTC2.Shared.Models.Interprocess;
using LTC2.Shared.Utils.Bootstrap.Interfaces;

namespace LTC2.Desktopclients.AvaloniaClient.ServiceTasks
{
    public class StartCalculatorServiceTask : IServiceTask
    {
        private readonly AppSettings _appSettings;
        private readonly ProfileManager _profileManager;
        private readonly StatusNotifier _statusNotifier;
        private readonly IBaseTranslationService _translationService;

        public StartCalculatorServiceTask(
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

            if (_appSettings.CalculatorApp != null)
            {
                var workingDirectory = Path.GetDirectoryName(_appSettings.CalculatorApp);

                var startInfo = new ProcessStartInfo();
                startInfo.WorkingDirectory = workingDirectory;
                startInfo.FileName = _appSettings.CalculatorApp;
                startInfo.CreateNoWindow = _appSettings.CalculatorNoWindow;

                var prof = _profileManager.Profile;
                startInfo.Arguments = $"{_appSettings.CalculatorAppParameters} ppid:{Process.GetCurrentProcess().Id} prof:{prof.ID}";

                Process.Start(startInfo);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            return Task.CompletedTask;
        }

        private void SendStartStatus()
        {
            _statusNotifier.Initialize();

            var msg = new StatusMessage()
            {
                Origin = StatusMessage.ORG_CALCULATOR,
                Status = StatusMessage.STATUS_START,
                Message = _translationService.GetMessage("progress.status.start")
            };

            _statusNotifier.Notify(msg);
        }
    }
}
