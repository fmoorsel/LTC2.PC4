using LTC2.Shared.Stores.Interfaces;
using LTC2.Shared.Stores.Stores;
using Microsoft.Extensions.DependencyInjection;

namespace LTC2.Shared.Stores.Bootstrap.Extensions
{
    public static class BootstrapStoresExtensions
    {
        public static IServiceCollection AddStores<TStore>(this IServiceCollection services) where TStore : class, ISessionStore
        {
            services.AddSingleton<ISessionStore, TStore>();

            return services;
        }

        public static IServiceCollection AddStores(this IServiceCollection services)
        {
            return services.AddStores<FileSessionStore>();
        }
    }
}
