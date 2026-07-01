using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace LTC2.Desktopclients.MauiClient.WinUI
{
    public partial class App : MauiWinUIApplication
    {
        public App()
        {
            this.InitializeComponent();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
