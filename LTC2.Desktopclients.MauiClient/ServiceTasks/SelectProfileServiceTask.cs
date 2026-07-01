using System.Threading.Tasks;
using LTC2.Desktopclients.MauiClient.Interfaces;
using LTC2.Desktopclients.MauiClient.Services;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace LTC2.Desktopclients.MauiClient.ServiceTasks
{
    public class SelectProfileServiceTask : IFirstServiceTask, IInterruptable
    {
        public bool ShouldStop { get; set; }

        private readonly ISelectProfilePageFactory _selectProfilePageFactory;
        private readonly ProfileManager _profileManager;

        public SelectProfileServiceTask(
            ISelectProfilePageFactory selectProfilePageFactory,
            ProfileManager profileManager)
        {
            _selectProfilePageFactory = selectProfilePageFactory;
            _profileManager = profileManager;
        }

        public async Task ExecuteAsync()
        {
            var profiles = _profileManager.GetProfiles();

            if (profiles.Count == 1)
            {
                _profileManager.Profile = profiles[0];
                return;
            }

            if (profiles.Count == 0)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var page = _selectProfilePageFactory.Create();
                    await Application.Current.MainPage.Navigation.PushModalAsync(page);
                    await page.WaitForClose();
                });
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var page = _selectProfilePageFactory.Create();
                await Application.Current.MainPage.Navigation.PushModalAsync(page);
                await page.WaitForClose();
            });
        }

        public Task StopAsync() => Task.CompletedTask;
    }
}
