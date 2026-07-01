using LTC2.Desktopclients.MauiProfileManager.Controls;
using LTC2.Desktopclients.MauiProfileManager.Factories;
using LTC2.Desktopclients.MauiProfileManager.Interfaces;
using LTC2.Desktopclients.MauiProfileManager.Models;
using LTC2.Desktopclients.MauiProfileManager.Pages;
using LTC2.Desktopclients.MauiProfileManager.Services;
using LTC2.Desktopclients.MauiProfileManager.ServiceTasks;
using LTC2.Shared.BaseMessages.Interfaces;
using LTC2.Shared.BaseMessages.Services;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.Repositories.Interfaces;
using LTC2.Shared.Repositories.Repositories;
using LTC2.Shared.Secrets.Interfaces;
using LTC2.Shared.Secrets.Vaults;
using LTC2.Shared.Utils.Bootstrap.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using System.Diagnostics;
using System.IO;

namespace LTC2.Desktopclients.MauiProfileManager
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>();

            builder.ConfigureMauiHandlers(handlers =>
            {
#if WINDOWS
                handlers.AddHandler<Ltc2WebView, LTC2.Desktopclients.MauiProfileManager.Platforms.Windows.Ltc2WebViewHandler>();
#endif
            });

            var config = GetConfig();

            var appSettings = new AppSettings();
            config.GetSection("AppSettings").Bind(appSettings);

            var genericSettings = new GenericSettings();
            config.GetSection("GenericSettings").Bind(genericSettings);

            builder.Services.AddSingleton(appSettings);
            builder.Services.AddSingleton(genericSettings);

            builder.Services.AddSingleton<Worker>();
            builder.Services.AddSingleton<StatusNotifier>();
            builder.Services.AddSingleton<ApplicationManager>();

            builder.Services.AddTransient<ProfileManagerPage>();
            builder.Services.AddTransient<TesterPage>();

            builder.Services.AddSingleton<ISecretsVault, WindowsSecretsVault>();
            builder.Services.AddSingleton<IDesktopProfileRepository, DesktopProfileRepository>();
            builder.Services.AddSingleton<ITesterPageFactory, TesterPageFactory>();

            builder.Services.AddSingleton<IServiceTask, InitWebappMonitorTask>();
            builder.Services.AddSingleton<IServiceTask, StartWebappServiceTask>();

            builder.Services.AddSingleton<IBaseTranslationService, BaseTranslationService>();

            builder.Logging.AddDebug();

            return builder.Build();
        }

        private static IConfigurationRoot GetConfig()
        {
            var processModule = Process.GetCurrentProcess().MainModule;
            var appSettingsFolder = Path.GetDirectoryName(processModule?.FileName) ?? System.AppContext.BaseDirectory;

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
    }
}
