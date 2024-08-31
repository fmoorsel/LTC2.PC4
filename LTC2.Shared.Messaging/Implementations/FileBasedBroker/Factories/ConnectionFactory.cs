using LTC2.Shared.Messaging.Implementations.FileBasedBroker.Interfaces;
using LTC2.Shared.Messaging.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace LTC2.Shared.Messaging.Implementations.FileBasedBroker.Factories
{
    public class ConnectionFactory : IBrokerConnectionFactory<Connection>
    {
        private readonly IServiceProvider _serviceProvider;

        public ConnectionFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Connection CreateBrokerConnection(string connectionString)
        {
            var logger = _serviceProvider.GetService<ILogger<Connection>>();
            var folderMonitorFactory = _serviceProvider.GetService<IFolderMonitorFactory>();

            return new Connection(logger, connectionString, folderMonitorFactory, _serviceProvider);
        }
    }
}
