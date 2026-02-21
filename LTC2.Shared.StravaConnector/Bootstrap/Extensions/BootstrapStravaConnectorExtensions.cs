using LTC2.Shared.Common.Interfaces;
using LTC2.Shared.Models.Domain;
using LTC2.Shared.Stores.Bootstrap.Extensions;
using LTC2.Shared.Stores.Interfaces;
using LTC2.Shared.Stores.Stores;
using LTC2.Shared.StravaConnector.Interfaces;
using LTC2.Shared.StravaConnector.Proxies;
using Microsoft.Extensions.DependencyInjection;

namespace LTC2.Shared.StravaConnector.Bootstrap.Extensions
{
    public static class BootstrapExtensions
    {
        public static IServiceCollection AddStravaConnector<TStore>(this IServiceCollection services) where TStore : class, ISessionStore
        {
            services.AddSingleton<IStravaConnector, Connector.StravaConnector>();
            services.AddKeyedSingleton<IConnector>(ConnectorSource.Strava,
                (sp, _) => sp.GetRequiredService<IStravaConnector>());
            services.AddSingleton<IStravaHttpProxy, StravaHttpProxy>();
            services.AddStores<TStore>();

            return services;
        }

        public static IServiceCollection AddStravaConnector(this IServiceCollection services)
        {
            return services.AddStravaConnector<FileSessionStore>();
        }
    }
}
