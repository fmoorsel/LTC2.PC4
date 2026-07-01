using LTC2.Desktopclients.MauiProfileManager.Models;
using LTC2.Desktopclients.MauiProfileManager.Services;
using LTC2.Shared.Models.Interprocess;
using Microsoft.Extensions.Logging;

namespace LTC2.Desktopclients.MauiProfileManager.ServiceTasks
{
    public class InitWebappMonitorTask : AbstractInitMonitorTask
    {
        public InitWebappMonitorTask(
            AppSettings appSettings,
            StatusNotifier statusNotifier,
            ILogger<InitWebappMonitorTask> logger) : base(appSettings, statusNotifier, logger) { }

        protected override string GetPipeNameParameter() => _appSettings.WebAppParameters;
        protected override string GetOrigin() => StatusMessage.ORG_WEBAPP;
    }
}
