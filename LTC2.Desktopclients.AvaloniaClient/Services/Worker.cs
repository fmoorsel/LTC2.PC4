using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LTC2.Desktopclients.AvaloniaClient.Interfaces;
using LTC2.Shared.Utils.Bootstrap.Interfaces;
using Microsoft.Extensions.Logging;

namespace LTC2.Desktopclients.AvaloniaClient.Services
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
            _logger.LogInformation("Starting worker");

            if (_serviceTasks != null && _serviceTasks.Count() > 0)
            {
                var mainTask = _serviceTasks.FirstOrDefault(s => s is IMainServiceTask) as IMainServiceTask;

                if (mainTask != null)
                {
                    mainTask.OnReady += ExecuteNonMainTasks;
                    await mainTask.ExecuteAsync();
                }
            }
        }

        public async Task Stop()
        {
            if (_serviceTasks != null && _serviceTasks.Count() > 0)
            {
                var firstTasks = _serviceTasks.Where(t => !(t is IFirstServiceTask)).Reverse();
                var tasks = _serviceTasks.Where(t => !(t is IMainServiceTask) && !(t is IFirstServiceTask)).Reverse();

                foreach (var task in tasks)
                {
                    await task.StopAsync();
                }

                foreach (var task in firstTasks)
                {
                    await task.StopAsync();
                }

                var mainTask = _serviceTasks.FirstOrDefault(s => s is IMainServiceTask) as IMainServiceTask;

                if (mainTask != null)
                {
                    await mainTask.StopAsync();
                }
            }
        }

        private async void ExecuteNonMainTasks(object sender, EventArgs e)
        {
            if (_serviceTasks != null && _serviceTasks.Count() > 0)
            {
                foreach (var task in _serviceTasks.Where(t => (t is IFirstServiceTask)))
                {
                    await task.ExecuteAsync();

                    if (task is IInterruptable interruptable && interruptable.ShouldStop)
                    {
                        Environment.Exit(0);
                    }
                }

                foreach (var task in _serviceTasks.Where(t => !(t is IMainServiceTask) && !(t is IFirstServiceTask)))
                {
                    await task.ExecuteAsync();

                    if (task is IInterruptable interruptable && interruptable.ShouldStop)
                    {
                        Environment.Exit(0);
                    }
                }
            }
        }
    }
}
