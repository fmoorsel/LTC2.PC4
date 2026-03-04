using LTC2.Shared.Common.Interfaces;
using LTC2.Shared.Models.Domain;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LTC2.Shared.Common.Factories
{
    public class ConnectorFactory : IConnectorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ConnectorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IConnector Create(ConnectorSource source)
            => _serviceProvider.GetRequiredKeyedService<IConnector>(source);
    }
}
