using LTC2.Desktopclients.MauiClient.Models;
using LTC2.Desktopclients.MauiClient.Services;
using LTC2.Shared.Models.Interprocess;
using Microsoft.Extensions.Logging;

namespace LTC2.Desktopclients.MauiClient.ServiceTasks
{
    public class InitCalculatorMonitorTask : AbstractInitMonitorTask
    {
        public InitCalculatorMonitorTask(
            AppSettings appSettings,
            StatusNotifier statusNotifier,
            ILogger<InitCalculatorMonitorTask> logger) : base(appSettings, statusNotifier, logger)
        {
        }

        protected override string GetPipeNameParameter() => _appSettings.CalculatorAppParameters;
        protected override string GetOrigin() => StatusMessage.ORG_CALCULATOR;
    }
}
