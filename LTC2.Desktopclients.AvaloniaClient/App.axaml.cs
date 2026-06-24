using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using LTC2.Desktopclients.AvaloniaClient.Models;
using LTC2.Desktopclients.AvaloniaClient.Services;
using LTC2.Desktopclients.AvaloniaClient.Windows;
using Microsoft.Extensions.DependencyInjection;

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

    public async void ShowAboutHandler(object sender, EventArgs e)
    {
        var aboutBox = Program.ApplicationHost.Services.GetRequiredService<AboutBoxWindow>();
        var mainWindow = (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        var windows = (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Windows.ToList();

        if (windows?.Find(w => w is AboutBoxWindow) != null)
        {
            return;
        }

        var selectedWindow = windows?.Find(w => w is SelectProfileWindow);

        if (selectedWindow != null)
        {
            await aboutBox.ShowDialog(selectedWindow);
        }
        else if (mainWindow != null)
        {
            await aboutBox.ShowDialog(mainWindow);
        }
    }
}
