using LTC2.Shared.Messaging.Implementations.FileBasedBroker;
using LTC2.Shared.Messaging.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LTC2.Shared.Messaging.Factories
{
    public class BrokerFactrory : IBrokerFactory<Message>
    {
        private IServiceProvider _serviceProvider;

        public BrokerFactrory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        public IBroker<Message> CreateBroker()
        {
            return _serviceProvider.GetService<IBroker<Message>>();
        }
    }
}
