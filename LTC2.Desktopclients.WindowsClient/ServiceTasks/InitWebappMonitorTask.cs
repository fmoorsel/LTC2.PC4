using LTC2.Desktopclients.WindowsClient.Models;
using LTC2.Desktopclients.WindowsClient.Services;
using LTC2.Shared.Models.Interprocess;

namespace LTC2.Desktopclients.WindowsClient.ServiceTasks
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
