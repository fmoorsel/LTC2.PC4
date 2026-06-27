using Avalonia;
using LTC2.Desktopclients.AvaloniaProfileManager.Factories;
using LTC2.Desktopclients.AvaloniaProfileManager.Interfaces;
using LTC2.Desktopclients.AvaloniaProfileManager.Models;
using LTC2.Desktopclients.AvaloniaProfileManager.Services;
using LTC2.Desktopclients.AvaloniaProfileManager.ServiceTasks;
using LTC2.Desktopclients.AvaloniaProfileManager.Windows;
using LTC2.Shared.BaseMessages.Interfaces;
using LTC2.Shared.BaseMessages.Services;
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

namespace LTC2.Desktopclients.AvaloniaProfileManager
{
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
                services.AddSingleton(CreateApplicationManager());

                services.AddTransient<ProfileManagerWindow>();
                services.AddTransient<TesterWindow>();

                services.AddSingleton<ISecretsVault, WindowsSecretsVault>();
                services.AddSingleton<IDesktopProfileRepository, DesktopProfileRepository>();
                services.AddSingleton<ITesterWindowFactory, TesterWindowFactory>();

                services.AddSingleton<IServiceTask, StartAvalonia>();
                services.AddSingleton<IServiceTask, InitWebappMonitorTask>();
                services.AddSingleton<IServiceTask, StartWebappServiceTask>();

                services.AddSingleton<IBaseTranslationService, BaseTranslationService>();
            });

            return hostBuilder;
        }

        private static void AddConfiguration(IServiceCollection services)
        {
            var configuration = GetConfig();

            var appSettings = new AppSettings();
            configuration.GetSection("AppSettings").Bind(appSettings);

            var genericSettings = new GenericSettings();
            configuration.GetSection("GenericSettings").Bind(genericSettings);

            services.AddSingleton(appSettings);
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
}
