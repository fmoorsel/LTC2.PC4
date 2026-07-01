using LTC2.Desktopclients.MauiProfileManager.Interfaces;
using LTC2.Shared.Utils.Bootstrap.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LTC2.Desktopclients.MauiProfileManager.Services
{
    public class Worker
    {
        private readonly IEnumerable<IServiceTask> _serviceTasks;
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger, IEnumerable<IServiceTask> serviceTasks)
        {
            _serviceTasks = serviceTasks;
            _logger = logger;
        }

        public async Task Execute()
        {
            _logger.LogInformation("Starting profile manager worker");

            if (_serviceTasks == null || !_serviceTasks.Any()) return;

            foreach (var task in _serviceTasks.Where(t => t is IFirstServiceTask))
            {
                await task.ExecuteAsync();
                if (task is IInterruptable interruptable && interruptable.ShouldStop)
                {
                    Environment.Exit(0);
                    return;
                }
            }

            foreach (var task in _serviceTasks.Where(t => !(t is IFirstServiceTask)))
            {
                await task.ExecuteAsync();
                if (task is IInterruptable interruptable && interruptable.ShouldStop)
                {
                    Environment.Exit(0);
                    return;
                }
            }
        }

        public async Task Stop()
        {
            if (_serviceTasks == null) return;

            var tasks = _serviceTasks.Where(t => !(t is IFirstServiceTask)).Reverse();
            foreach (var task in tasks) await task.StopAsync();

            var firstTasks = _serviceTasks.Where(t => t is IFirstServiceTask).Reverse();
            foreach (var task in firstTasks) await task.StopAsync();
        }
    }
}
