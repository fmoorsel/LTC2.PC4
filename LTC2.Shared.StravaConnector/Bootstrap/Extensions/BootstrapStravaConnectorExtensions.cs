using LTC2.Shared.StravaConnector.Interfaces;
using LTC2.Shared.StravaConnector.Proxies;
using LTC2.Shared.StravaConnector.Stores;
using Microsoft.Extensions.DependencyInjection;

namespace LTC2.Shared.StravaConnector.Bootstrap.Extensions
{
    public static class BootstrapExtensions
    {
        public static IServiceCollection AddStravaConnector<TStore>(this IServiceCollection services) where TStore : class, ISessionStore
        {
            services.AddSingleton<IStravaConnector, Connector.StravaConnector>();
            services.AddSingleton<IStravaHttpProxy, StravaHttpProxy>();
            services.AddSingleton<ISessionStore, TStore>();

            return services;
        }

        public static IServiceCollection AddStravaConnector(this IServiceCollection services)
        {
            services.AddStravaConnector<FileSessionStore>();

            return services;
        }
    }
}
