using Avalonia;
using Avalonia.Controls;

namespace LTC2.Desktopclients.AvaloniaProfileManager.Services
{
    public class ApplicationManager
    {
        public AppBuilder AppBuilder { get; }
        public Window MainWindow { get; set; }

        public ApplicationManager(AppBuilder appBuilder)
        {
            AppBuilder = appBuilder;
        }
    }
}
