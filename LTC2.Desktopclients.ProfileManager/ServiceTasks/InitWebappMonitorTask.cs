using LTC2.Desktopclients.ProfileManager.Models;
using LTC2.Desktopclients.ProfileManager.Services;
using LTC2.Shared.Models.Interprocess;

namespace LTC2.Desktopclients.ProfileManager.ServiceTasks
{
    public class InitWebappMonitorTask : AbstractInitMonitorTask
    {
        public InitWebappMonitorTask(AppSettings appSettings, StatusNotifier statusNotifier) : base(appSettings, statusNotifier)
        {

        }

        protected override string GetPipnameParameter()
        {
            return _appSettings.WebAppParameters;
        }

        protected override string GetOrigin()
        {
            return StatusMessage.ORG_WEBAPP;
        }
    }
}
