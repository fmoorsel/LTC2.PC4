using LTC2.Shared.Common.Factories;
using LTC2.Shared.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LTC2.Shared.Common.Bootstrap.Extensions
{
    public static class BootstrapConnectorFactoryExtensions
    {
        public static IServiceCollection AddConnectorFactory(this IServiceCollection services)
        {
            services.AddSingleton<IConnectorFactory, ConnectorFactory>();
            return services;
        }
    }
}
