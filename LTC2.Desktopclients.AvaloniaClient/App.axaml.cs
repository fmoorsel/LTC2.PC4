using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using LTC2.Desktopclients.AvaloniaClient.Models;
using LTC2.Desktopclients.AvaloniaClient.Services;
using LTC2.Desktopclients.AvaloniaClient.Windows;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace LTC2.Desktopclients.AvaloniaClient;

public partial class App : Application
{
    public static EventHandler OnReady { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var settings = Program.ApplicationHost.Services.GetRequiredService<AppSettings>();

        if (settings.LogFolder != null && !Directory.Exists(settings.LogFolder))
        {
            Directory.CreateDirectory(settings.LogFolder);
        }

        var mainWindow = Program.ApplicationHost.Services.GetRequiredService<StartWindow>();
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
