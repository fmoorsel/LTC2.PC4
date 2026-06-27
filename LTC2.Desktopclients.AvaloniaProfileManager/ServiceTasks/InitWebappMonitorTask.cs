using LTC2.Desktopclients.AvaloniaProfileManager.Models;
using LTC2.Desktopclients.AvaloniaProfileManager.Services;
using LTC2.Shared.Models.Interprocess;
using Microsoft.Extensions.Logging;

namespace LTC2.Desktopclients.AvaloniaProfileManager.ServiceTasks
{
    public class InitWebappMonitorTask : AbstractInitMonitorTask
    {
        public InitWebappMonitorTask(
            AppSettings appSettings,
            StatusNotifier statusNotifier,
            ILogger<InitWebappMonitorTask> logger) : base(appSettings, statusNotifier, logger)
        {
        }

        protected override string GetPipeNameParameter()
        {
            return _appSettings.WebAppParameters;
        }

        protected override string GetOrigin()
        {
            return StatusMessage.ORG_WEBAPP;
        }
    }
}
