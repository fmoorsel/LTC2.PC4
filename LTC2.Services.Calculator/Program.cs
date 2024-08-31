using LTC2.Services.Calculator.Calculator;
using LTC2.Services.Calculator.Interfaces;
using LTC2.Services.Calculator.Models;
using LTC2.Services.Calculator.Services;
using LTC2.Services.Calculator.ServiceTasks;
using LTC2.Shared.Messaging.Implementations.FileBasedBroker.Extensions;
using LTC2.Shared.Repositories.Interfaces;
using LTC2.Shared.Repositories.Mapdefinitions;
using LTC2.Shared.Repositories.Repositories;
using LTC2.Shared.Secrets.Interfaces;
using LTC2.Shared.Secrets.Vaults;
using LTC2.Shared.SpatiaLiteRepository.Repositories;
using LTC2.Shared.StravaConnector.Bootstrap.Extensions;
using LTC2.Shared.StravaConnector.Interfaces;
using LTC2.Shared.StravaConnector.Stores;
using LTC2.Shared.Utils.Bootstrap.Extensions;
using LTC2.Shared.Utils.Bootstrap.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;

namespace LTC2.Services.Calculator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception)
            {
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var appSettings = GetAppSettings();

            var hostBuilder = Host.CreateDefaultBuilder(args);

            var settingsService = new CalculatorSettingsService();

            hostBuilder.ConfigureServices((services) =>
            {
                services.AddHostedService<Worker>();

                services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(GetSeriLogger(), true));

                services.AddSettings(settingsService);

                services.AddSingleton<IServiceTask, InitStravaPropertiesTask>();
                services.AddSingleton<IServiceTask, InitStatusPublisherTask>();
                services.AddSingleton<IServiceTask, InitMapRepositoryTask>();
                services.AddSingleton<IServiceTask, InitScoreCalculatorTask>();
                services.AddSingleton<IServiceTask, InitFolderScannerTask>();
                services.AddSingleton<IServiceTask, InitIntermediateResultRepository>();

                services.AddSingleton<IMapRepository, SpatiaLiteMapRepository>();

                //services.AddSingleton<IMapRepository, SqlServerMapRepository>();
                services.AddSingleton<IPlacesRepository, PlacesRepository>();
                services.AddSingleton<IScoreCalculator, ScoreCalculator>();
                services.AddSingleton<IIntermediateResultsRepository, IntermediateResultsRepository>();
                //services.AddSingleton<IScoresRepository, ScoreRepository>();

                services.AddSingleton<IScoresRepository, SqliteScoreRepository>();

                services.AddSingleton<ISecretsVault, WindowsSecretsVault>();
                services.AddSingleton<IDesktopProfileRepository, DesktopProfileRepository>();

                services.AddSingleton<SpatiaLiteRepository>();
                services.AddSingleton<StatusNotifier>();

                services.AddSingleton(appSettings);

                services.AddSingleton<ISessionStore, FileSessionStore>();

                services.AddStravaConnector();
                services.AddFileBasedBroker();
            });

            return hostBuilder;
        }

        private static AppSettings GetAppSettings()
        {
            var processModule = Process.GetCurrentProcess().MainModule;
            var appSettingsFolder = Path.GetDirectoryName(processModule?.FileName);

            var configuration = new ConfigurationBuilder().SetBasePath(appSettingsFolder)
                        .AddJsonFile("appsettings.json", true, true)
                        .Build();

            return configuration.GetSection("AppSettings").Get<AppSettings>();
        }

        private static ILogger GetSeriLogger()
        {
            var configuration = new ConfigurationBuilder()
                                        .SetBasePath(Directory.GetCurrentDirectory())
                                        .AddJsonFile("serlilogsettings.json")
                                        .Build();

            Log.Logger = new LoggerConfiguration()
                                .ReadFrom.Configuration(configuration)
                                .CreateLogger();

            return Log.Logger;
        }
    }


}

