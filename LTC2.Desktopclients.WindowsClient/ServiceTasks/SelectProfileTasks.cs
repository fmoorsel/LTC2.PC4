using LTC2.Desktopclients.WindowsClient.Forms;
using LTC2.Desktopclients.WindowsClient.Interfaces;
using LTC2.Desktopclients.WindowsClient.Services;

namespace LTC2.Desktopclients.WindowsClient.ServiceTasks
{
    public class SelectProfileTasks : IFirstServiceTask, IInterruptable
    {
        public bool ShouldStop { get; set; }

        private readonly SelectProfileForm _selectProfileForm;
        private readonly SplashScreen _splashScreen;
        private readonly ProfileManager _profileManager;

        public SelectProfileTasks(
            SelectProfileForm selectProfileForm,
            SplashScreen splashScreen,
            ProfileManager profileManager)
        {
            _selectProfileForm = selectProfileForm;
            _splashScreen = splashScreen;
            _profileManager = profileManager;
        }

        public Task ExecuteAsync()
        {
            var profiles = _profileManager.GetProfiles();

            if (profiles.Count == 1)
            {
                _profileManager.Profile = profiles[0];
            }
            else
            {
                _selectProfileForm.TopMost = true;
                _selectProfileForm.ShowDialog();
                _selectProfileForm.TopMost = false;

                ShouldStop = _selectProfileForm.ShouldProgress;

                if (!ShouldStop)
                {
                    _splashScreen.TopMost = true;
                }
            }

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            return Task.CompletedTask;
        }
    }
}
