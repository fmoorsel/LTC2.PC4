using System.Threading.Tasks;
using LTC2.Desktopclients.AvaloniaClient.Interfaces;
using LTC2.Desktopclients.AvaloniaClient.Services;

namespace LTC2.Desktopclients.AvaloniaClient.ServiceTasks
{
    public class SelectProfileServiceTask : IFirstServiceTask, IInterruptable
    {
        public bool ShouldStop { get; set; }

        private readonly ISelectProfileWindowFactory _selectProfileWindowFactory;
        private readonly ProfileManager _profileManager;
        private readonly ApplicationManager _applicationManager;

        public SelectProfileServiceTask(
            ApplicationManager applicationManager,
            ISelectProfileWindowFactory selectProfileWindowFactory,
            ProfileManager profileManager)
        {
            _selectProfileWindowFactory = selectProfileWindowFactory;
            _profileManager = profileManager;
            _applicationManager = applicationManager;
        }

        public async Task ExecuteAsync()
        {
            var profiles = _profileManager.GetProfiles();

            if (profiles.Count == 1)
            {
                _profileManager.Profile = profiles[0];
            }
            else
            {
                var mainWindow = _applicationManager.MainWindow;

                if (mainWindow == null)
                {
                    if (profiles.Count > 1)
                    {
                        _profileManager.Profile = profiles[0];
                    }

                    return;
                }

                var selectProfileWindow = _selectProfileWindowFactory.Create();
                await selectProfileWindow.ShowDialog(mainWindow);
            }
        }

        public Task StopAsync()
        {
            return Task.CompletedTask;
        }
    }
}
