using LTC2.Shared.Messaging.Factories;
using LTC2.Shared.Messaging.Implementations.FileBasedBroker.Factories;
using LTC2.Shared.Messaging.Implementations.FileBasedBroker.Interfaces;
using LTC2.Shared.Messaging.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LTC2.Shared.Messaging.Implementations.FileBasedBroker.Extensions
{
    public static class FileBasedBrokerExtensions
    {
        public static IServiceCollection AddFileBasedBroker(this IServiceCollection services)
        {
            services.AddSingleton<IBrokerFactory<Message>, BrokerFactrory>();
            services.AddSingleton<IBroker<Message>, Broker>();
            services.AddSingleton<IBrokerConnectionFactory<Connection>, ConnectionFactory>();
            services.AddSingleton<IFolderMonitorFactory, FolderMonitorFactory>();

            return services;

        }
    }
}
