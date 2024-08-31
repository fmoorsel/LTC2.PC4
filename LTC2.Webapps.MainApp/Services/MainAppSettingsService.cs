
using LTC2.Shared.Models.Settings;
using LTC2.Shared.Utils.Bootstrap.Interfaces;
using LTC2.Webapps.MainApp.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace LTC2.Webapps.MainApp.Services
{
    public class MainAppSettingsService : ISettingsService
    {
        private readonly ConcurrentDictionary<Type, object> _allSettings;

        public MainAppSettingsService()
        {
            _allSettings = new ConcurrentDictionary<Type, object>();
        }

        public Dictionary<Type, object> GetSettings()
        {
            var result = new Dictionary<Type, object>();

            GetSettingsFromConfig<GenericSettings>("GenericSettings", result);
            GetSettingsFromConfig<StravaHttpProxySettings>("StravaHttpProxySettings", result);
            GetSettingsFromConfig<AuthorizationSettings>("AuthorizationSettings", result);
            GetSettingsFromConfig<CalculatorSettings>("CalculatorSettings", result);
            GetSettingsFromConfig<MainClientSettings>("MainClientSettings", result);

            foreach (var key in result.Keys)
            {
                _allSettings[key] = result[key];
            }

            return result;
        }

        public TSettingsType GetSettings<TSettingsType>() where TSettingsType : class
        {
            var type = typeof(TSettingsType);

            if (_allSettings.ContainsKey(type))
            {
                var instance = _allSettings[type];
                return (TSettingsType)instance;
            }

            return default(TSettingsType);
        }

        private void GetSettingsFromConfig<T>(string sectionName, Dictionary<Type, object> settings) where T : class
        {
            var section = GetConfigurationSection(sectionName);

            if (section != null)
            {
                var settingsValue = section.Get<T>();

                if (settingsValue != null)
                {
                    settings.Add(typeof(T), settingsValue);
                }
            }
        }

        private IConfigurationSection GetConfigurationSection(string section)
        {
            var processModule = Process.GetCurrentProcess().MainModule;
            var appSettingsFolder = Path.GetDirectoryName(processModule?.FileName);

            var configuration = new ConfigurationBuilder().SetBasePath(appSettingsFolder)
                        .AddJsonFile("appsettings.json", true, true)
                        .Build();

            if (configuration != null)
            {
                return configuration.GetSection(section);
            }
            else
            {
                return null;
            }
        }
    }
}
