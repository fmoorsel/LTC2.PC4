using LTC2.Shared.RideWithGpsConnector.Interfaces;
using LTC2.Shared.RideWithGpsConnector.Proxies;
using Microsoft.Extensions.DependencyInjection;

namespace LTC2.Shared.RideWithGpsConnector.Bootstrap.Extensions
{
    public static class BootstrapRideWithGpsConnectorExtensions
    {
        public static IServiceCollection AddRideWithGpsConnector(this IServiceCollection services)
        {
            services.AddSingleton<IRideWithGpsConnector, Connector.RideWithGpsConnector>();
            services.AddSingleton<IRideWithGpsHttpProxy, RideWithGpsHttpProxy>();
            return services;
        }
    }
}
