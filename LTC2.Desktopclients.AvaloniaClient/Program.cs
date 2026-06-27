using Avalonia;
using LTC2.Desktopclients.AvaloniaClient.Factories;
using LTC2.Desktopclients.AvaloniaClient.Interfaces;
using LTC2.Desktopclients.AvaloniaClient.Models;
using LTC2.Desktopclients.AvaloniaClient.Services;
using LTC2.Desktopclients.AvaloniaClient.ServiceTasks;
using LTC2.Desktopclients.AvaloniaClient.Windows;
using LTC2.Shared.BaseMessages.Interfaces;
using LTC2.Shared.BaseMessages.Services;
using LTC2.Shared.Http.Interfaces;
using LTC2.Shared.Http.Proxies;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.Repositories.Interfaces;
using LTC2.Shared.Repositories.Repositories;
using LTC2.Shared.Secrets.Interfaces;
using LTC2.Shared.Secrets.Vaults;
using LTC2.Shared.Utils.Bootstrap.Interfaces;
using LTC2.Shared.Utils.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.IO;

namespace LTC2.Desktopclients.AvaloniaClient;

public static class Program
{
    public static IHost ApplicationHost { get; private set; }

    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            ProcessUtils.EnsureOnlyOneProcess();

            ApplicationHost = CreateHostBuilder().Build();

            var worker = ApplicationHost.Services.GetRequiredService<Worker>();

            worker.Execute().GetAwaiter().GetResult();
            worker.Stop().GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static IHostBuilder CreateHostBuilder()
    {
        var hostBuilder = Host.CreateDefaultBuilder();

        hostBuilder.ConfigureServices((services) =>
        {
            AddConfiguration(services);

            services.AddSingleton<Worker>();
            services.AddSingleton<StatusNotifier>();
            services.AddSingleton<MultiSportManager>();
            services.AddSingleton(CreateApplicationManager());

            services.AddTransient<StartWindow>();
            services.AddTransient<BrowserWindow>();
            services.AddTransient<UpdateWindow>();
            services.AddTransient<SelectActivitiesWindow>();
            services.AddTransient<SelectProfileWindow>();
            services.AddSingleton<RoutePlannerWindow>();

            services.AddSingleton<ISecretsVault, WindowsSecretsVault>();
            services.AddSingleton<WebViewConnector>();
            services.AddSingleton<ProfileManager>();
            services.AddSingleton<ProfileManagerStarter>();
            services.AddSingleton<IDesktopProfileRepository, DesktopProfileRepository>();
            services.AddSingleton<ISelectProfileWindowFactory, SelectProfileWindowFactory>();
            services.AddSingleton<ISelectActivitiesWindowFactory, SelectActivitiesWindowFactory>();
            services.AddSingleton<ILTC2HttpProxy, LTC2HttpProxy>();

            services.AddSingleton<IServiceTask, StartAvalonia>();
            services.AddSingleton<IServiceTask, ClearLoggingFoldersTasks>();
            services.AddSingleton<IServiceTask, SelectProfileServiceTask>();
            services.AddSingleton<IServiceTask, InitWebappMonitorTask>();
            services.AddSingleton<IServiceTask, InitCalculatorMonitorTask>();
            services.AddSingleton<IServiceTask, StartWebappServiceTask>();
            services.AddSingleton<IServiceTask, StartCalculatorServiceTask>();

            services.AddSingleton<IBaseTranslationService, BaseTranslationService>();
        });

        return hostBuilder;
    }

    private static void AddConfiguration(IServiceCollection services)
    {
        var configuration = GetConfig();

        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();
        var lTC2HttpProxySettings = configuration.GetSection("LTC2HttpProxySettings").Get<LTC2HttpProxySettings>();
        var genericSettings = configuration.GetSection("GenericSettings").Get<GenericSettings>();

        services.AddSingleton(appSettings);
        services.AddSingleton(lTC2HttpProxySettings);
        services.AddSingleton(genericSettings);
    }

    private static IConfigurationRoot GetConfig()
    {
        var processModule = Process.GetCurrentProcess().MainModule;
        var appSettingsFolder = Path.GetDirectoryName(processModule?.FileName) ?? string.Empty;

        if (File.Exists(Path.Combine(appSettingsFolder, "appsettings.Development.json")))
        {
            return new ConfigurationBuilder()
                .SetBasePath(appSettingsFolder)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile("appsettings.Development.json", true, true)
                .Build();
        }

        return new ConfigurationBuilder()
            .SetBasePath(appSettingsFolder)
            .AddJsonFile("appsettings.json", true, true)
            .Build();
    }

    private static ApplicationManager CreateApplicationManager()
    {
        var appBuilder = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

        return new ApplicationManager(appBuilder);
    }
}
