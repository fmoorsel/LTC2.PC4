using LTC2.Shared.Common.Interfaces;
using LTC2.Shared.Models.Domain;
using LTC2.Shared.RideWithGpsConnector.Interfaces;
using LTC2.Shared.RideWithGpsConnector.Proxies;
using LTC2.Shared.Stores.Bootstrap.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace LTC2.Shared.RideWithGpsConnector.Bootstrap.Extensions
{
    public static class BootstrapRideWithGpsConnectorExtensions
    {
        public static IServiceCollection AddRideWithGpsConnector(this IServiceCollection services)
        {
            services.AddSingleton<IRideWithGpsConnector, Connector.RideWithGpsConnector>();
            services.AddKeyedSingleton<IConnector>(ConnectorSource.RideWithGps,
                (sp, _) => sp.GetRequiredService<IRideWithGpsConnector>());
            services.AddSingleton<IRideWithGpsHttpProxy, RideWithGpsHttpProxy>();
            services.AddStores();
            return services;
        }
    }
}
