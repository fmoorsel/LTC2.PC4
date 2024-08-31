using LTC2.Desktopclients.WindowsClient.Interfaces;
using LTC2.Shared.Utils.Bootstrap.Interfaces;
using Microsoft.Extensions.Logging;

namespace LTC2.Desktopclients.WindowsClient.Services
{
    public class Worker
    {
        private readonly IEnumerable<IServiceTask> _serviceTasks;
        private readonly IEnumerable<IPrerequisiteServiceTask> _prerequisiteserviceTasks;
        private readonly ILogger<Worker> _logger;
        public Worker(
            ILogger<Worker> logger,
            IEnumerable<IServiceTask> serviceTasks,
            IEnumerable<IPrerequisiteServiceTask> prerequisiteserviceTasks)
        {
            _serviceTasks = serviceTasks;
            _prerequisiteserviceTasks = prerequisiteserviceTasks;
            _logger = logger;
        }

        public void Execute()
        {
            _logger.LogInformation("Start control panel application");

            ExecutePrerequisiteServiceTasks();


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
                var firstTasks = _serviceTasks.Where(t => !(t is IFirstServiceTask)).Reverse();
                var tasks = _serviceTasks.Where(t => !(t is IMainServiceTask) && !(t is IFirstServiceTask)).Reverse();

                foreach (var task in tasks)
                {
                    task.StopAsync().Wait();
                }

                foreach (var task in firstTasks)
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

        private void ExecutePrerequisiteServiceTasks()
        {
            if (_prerequisiteserviceTasks != null && _prerequisiteserviceTasks.Count() > 0)
            {
                foreach (var task in _prerequisiteserviceTasks)
                {
                    task.ExecuteAsync().Wait();

                    if (task is IInterruptable)
                    {
                        var shouldStop = (task as IInterruptable).ShouldStop;

                        if (shouldStop)
                        {
                            Environment.Exit(0);
                        }
                    }
                }
            }
        }

        private void ExecuteNonMainTasks(object sender, EventArgs e)
        {
            if (_serviceTasks != null && _serviceTasks.Count() > 0)
            {
                foreach (var task in _serviceTasks.Where(t => (t is IFirstServiceTask)))
                {
                    task.ExecuteAsync().Wait();

                    if (task is IInterruptable)
                    {
                        var shouldStop = (task as IInterruptable).ShouldStop;

                        if (shouldStop)
                        {
                            Environment.Exit(0);
                        }
                    }
                }

                foreach (var task in _serviceTasks.Where(t => !(t is IMainServiceTask) && !(t is IFirstServiceTask)))
                {
                    task.ExecuteAsync().Wait();

                    if (task is IInterruptable)
                    {
                        var shouldStop = (task as IInterruptable).ShouldStop;

                        if (shouldStop)
                        {
                            Environment.Exit(0);
                        }
                    }
                }
            }
        }
    }
}
