using LTC2.Desktopclients.AvaloniaClient.Models;
using LTC2.Desktopclients.AvaloniaClient.Services;
using LTC2.Shared.Models.Interprocess;
using Microsoft.Extensions.Logging;

namespace LTC2.Desktopclients.AvaloniaClient.ServiceTasks
{
    public class InitCalculatorMonitorTask : AbstractInitMonitorTask
    {
        public InitCalculatorMonitorTask(
            AppSettings appSettings,
            StatusNotifier statusNotifier,
            ILogger<InitCalculatorMonitorTask> logger) : base(appSettings, statusNotifier, logger)
        {
        }

        protected override string GetPipeNameParameter()
        {
            return _appSettings.CalculatorAppParameters;
        }

        protected override string GetOrigin()
        {
            return StatusMessage.ORG_CALCULATOR;
        }
    }
}
