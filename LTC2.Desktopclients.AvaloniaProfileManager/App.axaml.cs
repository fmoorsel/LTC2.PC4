using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using LTC2.Desktopclients.AvaloniaProfileManager.Services;
using LTC2.Desktopclients.AvaloniaProfileManager.Windows;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LTC2.Desktopclients.AvaloniaProfileManager
{
    public partial class App : Application
    {
        public static EventHandler OnReady { get; set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var mainWindow = Program.ApplicationHost.Services.GetRequiredService<ProfileManagerWindow>();
            var applicationManager = Program.ApplicationHost.Services.GetRequiredService<ApplicationManager>();

            applicationManager.MainWindow = mainWindow;

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();

            Dispatcher.UIThread.Post(() => OnReady?.Invoke(this, EventArgs.Empty));
        }
    }
}
