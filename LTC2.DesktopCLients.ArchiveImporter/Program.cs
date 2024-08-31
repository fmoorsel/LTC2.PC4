using LTC2.DesktopClients.ArchiveImporter.Forms;
using LTC2.DesktopClients.ArchiveImporter.Models;
using LTC2.DesktopClients.ArchiveImporter.Services;
using LTC2.DesktopClients.ArchiveImporter.ServiceTasks;
using LTC2.Shared.Messages.Interfaces;
using LTC2.Shared.Messages.Services;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.Utils.Bootstrap.Interfaces;
using LTC2.Shared.Utils.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace LTC2.DesktopClients.ArchiveImporter
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ProcessUtils.EnsureOnlyOneProcess();

            ApplicationConfiguration.Initialize();

            var host = CreateHostBuilder().Build();
            var worker = host.Services.GetRequiredService<Worker>();

            // run the app
            worker.Execute();

            // clean up before exit
            worker.Stop();
        }

        private static IHostBuilder CreateHostBuilder()
        {
            var appSettings = GetAppSettings();

            var hostBuilder = Host.CreateDefaultBuilder();

            hostBuilder.ConfigureServices((services) =>
            {
                services.AddTransient<ImportForm>();

                AddConfiguration(services);

                services.AddSingleton<Worker>();
                services.AddSingleton<ArchiveProcessor>();

                services.AddSingleton<ITranslationService, TranslationService>();

                services.AddSingleton<IServiceTask, StartWinforms>();
            });

            return hostBuilder;
        }


        private static void AddConfiguration(IServiceCollection services)
        {
            var configuration = GetConfig();

            var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();
            var genericSettings = configuration.GetSection("GenericSettings").Get<GenericSettings>();

            services.AddSingleton(appSettings);
            services.AddSingleton(genericSettings);
        }

        private static AppSettings GetAppSettings()
        {
            var configuration = GetConfig();

            return configuration.GetSection("AppSettings").Get<AppSettings>();
        }

        private static IConfigurationRoot GetConfig()
        {
            var processModule = Process.GetCurrentProcess().MainModule;
            var appSettingsFolder = Path.GetDirectoryName(processModule?.FileName);

            var configuration = new ConfigurationBuilder().SetBasePath(appSettingsFolder)
                        .AddJsonFile("appsettings.json", true, true)
                        .Build();

            return configuration;
        }

    }
}