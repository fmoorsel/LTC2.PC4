using LTC2.Desktopclients.WindowsClient.Forms;
using LTC2.Desktopclients.WindowsClient.Models;
using LTC2.Desktopclients.WindowsClient.Services;
using LTC2.Desktopclients.WindowsClient.ServiceTasks;
using LTC2.Shared.Http.Interfaces;
using LTC2.Shared.Http.Proxies;
using LTC2.Shared.Messages.Interfaces;
using LTC2.Shared.Messages.Services;
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
using System.Diagnostics;

namespace LTC2.Desktopclients.WindowsClient
{
    public static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
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
                services.AddTransient<SplashScreen>();
                services.AddTransient<MainForm>();
                services.AddTransient<UpdateActivitiesForm>();
                services.AddTransient<SelectProfileForm>();

                AddConfiguration(services);

                services.AddSingleton<Worker>();
                services.AddSingleton<StatusNotifier>();
                services.AddSingleton<WebviewConnector>();
                services.AddSingleton<ProfileManager>();

                services.AddSingleton<ILTC2HttpProxy, LTC2HttpProxy>();
                services.AddSingleton<ISecretsVault, WindowsSecretsVault>();
                services.AddSingleton<IDesktopProfileRepository, DesktopProfileRepository>();
                services.AddSingleton<ITranslationService, TranslationService>();

                //services.AddSingleton<IPrerequisiteServiceTask, CheckTilesPrerequisiteTask>();

                services.AddSingleton<IServiceTask, SelectProfileTasks>();
                services.AddSingleton<IServiceTask, ClearLoggingFoldersTasks>();
                services.AddSingleton<IServiceTask, InitCalculatorMonitorTask>();
                services.AddSingleton<IServiceTask, InitWebappMonitorTask>();
                services.AddSingleton<IServiceTask, StartWebappServiceTask>();
                services.AddSingleton<IServiceTask, StartCalculatorServiceTask>();
                services.AddSingleton<IServiceTask, StartWinforms>();
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