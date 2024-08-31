using LTC2.DesktopClients.ArchiveImporter.Interfaces;
using LTC2.Shared.Utils.Bootstrap.Interfaces;
using Microsoft.Extensions.Logging;

namespace LTC2.DesktopClients.ArchiveImporter.Services
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

        public void Execute()
        {
            _logger.LogInformation("Start control panel application");

            if (_serviceTasks != null && _serviceTasks.Count() > 0)
            {
                var mainTask = _serviceTasks.FirstOrDefault(s => s is IMainServiceTask) as IMainServiceTask;

                if (mainTask != null)
                {
                    mainTask.OnReady += ExecuteNonMainTasks;
                    mainTask.ExecuteAsync().Wait();
                }
            }
        }

        public void Stop()
        {
            if (_serviceTasks != null && _serviceTasks.Count() > 0)
            {
                var tasks = _serviceTasks.Where(t => !(t is IMainServiceTask)).Reverse();

                foreach (var task in tasks)
                {
                    task.StopAsync().Wait();
                }

                var mainTask = _serviceTasks.FirstOrDefault(s => s is IMainServiceTask) as IMainServiceTask;

                if (mainTask != null)
                {
                    mainTask.StopAsync().Wait();
                }
            }
        }

        private void ExecuteNonMainTasks(object sender, EventArgs e)
        {
            if (_serviceTasks != null && _serviceTasks.Count() > 0)
            {
                foreach (var task in _serviceTasks.Where(t => !(t is IMainServiceTask)))
                {
                    task.ExecuteAsync().Wait();
                }
            }
        }
    }
}
