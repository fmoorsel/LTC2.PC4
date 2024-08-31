using LTC2.Shared.Utils.Bootstrap.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LTC2.Shared.Utils.Bootstrap.Extensions
{
    public static class BootstrapExtensions
    {
        public static IServiceCollection AddSettings(this IServiceCollection services, ISettingsService settingsService)
        {
            var settings = settingsService.GetSettings();

            foreach (var type in settings.Keys)
            {
                services.AddSingleton(type, settings[type]);
            }

            return services;

        }
    }
}
