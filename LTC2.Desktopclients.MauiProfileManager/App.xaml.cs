using LTC2.Desktopclients.MauiProfileManager.Pages;
using LTC2.Desktopclients.MauiProfileManager.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace LTC2.Desktopclients.MauiProfileManager
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var page = IPlatformApplication.Current.Services.GetRequiredService<ProfileManagerPage>();
            return new Window(new NavigationPage(page))
            {
                Title = "LTC2 Profile Manager",
                Width = 800,
                Height = 520,
                MaximumWidth = 800,
                MaximumHeight = 520
            };
        }

        protected override void OnStart()
        {
            base.OnStart();
            var worker = IPlatformApplication.Current.Services.GetRequiredService<Worker>();
            Task.Run(() => worker.Execute());
        }

        protected override void OnSleep()
        {
            base.OnSleep();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }
    }
}
