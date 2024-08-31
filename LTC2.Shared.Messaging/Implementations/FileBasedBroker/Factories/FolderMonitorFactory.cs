using LTC2.Shared.Messaging.Implementations.FileBasedBroker.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace LTC2.Shared.Messaging.Implementations.FileBasedBroker.Factories
{

    public class FolderMonitorFactory : IFolderMonitorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public FolderMonitorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IFolderMonitor CreateFolderMonitor(string folder)
        {
            var logger = _serviceProvider.GetService<ILogger<FolderMonitor>>();

            return new FolderMonitor(logger, folder);
        }
    }
}
