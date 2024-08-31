using LTC2.Shared.Utils.Bootstrap.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LTC2.Services.Calculator.Services
{
    public class Worker : BackgroundService
    {
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly IEnumerable<IServiceTask> _serviceTasks;
        private readonly ILogger<Worker> _logger;

        public Worker(IHostApplicationLifetime applicationLifetime, ILogger<Worker> logger, IEnumerable<IServiceTask> serviceTasks)
        {
            _applicationLifetime = applicationLifetime;
            _serviceTasks = serviceTasks;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Start calculator");

            _applicationLifetime.ApplicationStopping.Register(new Action(OnStopping));

            if (_serviceTasks != null && _serviceTasks.Count() > 0)
            {
                foreach (var task in _serviceTasks)
                {
                    await task.ExecuteAsync();
                }
            }
        }

        private void OnStopping()
        {
            _logger.LogInformation("About to stop calculator");

            if (_serviceTasks != null && _serviceTasks.Count() > 0)
            {
                foreach (var task in _serviceTasks.Reverse())
                {
                    try
                    {
                        Task.WaitAll(task.StopAsync());
                    }
                    catch (Exception)
                    {

                    }
                }
            }

            _logger.LogInformation("All tasks stopped");
        }
    }
}
