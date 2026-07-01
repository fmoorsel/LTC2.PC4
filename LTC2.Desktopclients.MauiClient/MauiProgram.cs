using System;
using LTC2.Desktopclients.MauiClient.Controls;
using LTC2.Desktopclients.MauiClient.Factories;
using LTC2.Desktopclients.MauiClient.Interfaces;
using LTC2.Desktopclients.MauiClient.Models;
using LTC2.Desktopclients.MauiClient.Pages;
using LTC2.Desktopclients.MauiClient.ServiceTasks;
using LTC2.Desktopclients.MauiClient.Services;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

#if WINDOWS
using LTC2.Desktopclients.MauiClient.Platforms.Windows;
#endif

namespace LTC2.Desktopclients.MauiClient
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>();

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var appSettings = new AppSettings();
            config.GetSection("AppSettings").Bind(appSettings);
            builder.Services.AddSingleton(appSettings);

            var proxySettings = new LTC2HttpProxySettings();
            config.GetSection("LTC2HttpProxySettings").Bind(proxySettings);
            builder.Services.AddSingleton(proxySettings);

            var genericSettings = new GenericSettings();
            config.GetSection("GenericSettings").Bind(genericSettings);
            builder.Services.AddSingleton(genericSettings);

            builder.Services.AddSingleton<ISecretsVault>(sp => new WindowsSecretsVault(sp.GetRequiredService<GenericSettings>()));
            builder.Services.AddSingleton<IDesktopProfileRepository, DesktopProfileRepository>();

            builder.Services.AddSingleton<ILTC2HttpProxy, LTC2HttpProxy>();
            builder.Services.AddSingleton<IBaseTranslationService>(sp =>
            {
                var svc = new BaseTranslationService();
                svc.LoadMessagesForLanguage("nl");
                return svc;
            });

            builder.Services.AddSingleton<StatusNotifier>();
            builder.Services.AddSingleton<ProfileManager>();
            builder.Services.AddSingleton<MultiSportManager>();
            builder.Services.AddSingleton<WebViewConnector>();
            builder.Services.AddSingleton<ProfileManagerStarter>();
            builder.Services.AddSingleton<ApplicationManager>();
            builder.Services.AddSingleton<Worker>();

            builder.Services.AddTransient<StartPage>();
            builder.Services.AddTransient<BrowserPage>();
            builder.Services.AddTransient<UpdatePage>();
            builder.Services.AddTransient<SelectProfilePage>();
            builder.Services.AddTransient<SelectActivitiesPage>();
            builder.Services.AddTransient<RoutePlannerPage>();

            builder.Services.AddSingleton<ISelectProfilePageFactory, SelectProfilePageFactory>();
            builder.Services.AddSingleton<ISelectActivitiesPageFactory, SelectActivitiesPageFactory>();

            builder.Services.AddSingleton<IServiceTask, ClearLoggingFoldersTasks>();
            builder.Services.AddSingleton<IServiceTask, SelectProfileServiceTask>();
            builder.Services.AddSingleton<IServiceTask, StartWebappServiceTask>();
            builder.Services.AddSingleton<IServiceTask, InitWebappMonitorTask>();
            builder.Services.AddSingleton<IServiceTask, StartCalculatorServiceTask>();
            builder.Services.AddSingleton<IServiceTask, InitCalculatorMonitorTask>();

            builder.ConfigureMauiHandlers(handlers =>
            {
#if WINDOWS
                handlers.AddHandler<Ltc2WebView, Ltc2WebViewHandler>();
#endif
            });

            builder.Logging.AddDebug();

            return builder.Build();
        }
    }
}
