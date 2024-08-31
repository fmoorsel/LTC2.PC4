using LTC2.Services.Calculator.Models;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.Utils.Bootstrap.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace LTC2.Services.Calculator.Services
{
    public class CalculatorSettingsService : ISettingsService
    {
        public Dictionary<Type, object> GetSettings()
        {
            var result = new Dictionary<Type, object>();

            GetSettingsFromConfig<ElasticUploaderSettings>("ElasticUploaderSettings", result);
            GetSettingsFromConfig<CalculatorSettings>("CalculatorSettings", result);
            GetSettingsFromConfig<GenericSettings>("GenericSettings", result);
            GetSettingsFromConfig<StravaHttpProxySettings>("StravaHttpProxySettings", result);

            GetSettingsFromConfig<SpatiaLiteMapperSettings>("SpatiaLiteMapperSettings", result);

            return result;
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

        public TSettingsType GetSettings<TSettingsType>() where TSettingsType : class
        {
            throw new NotImplementedException();
        }
    }
}
