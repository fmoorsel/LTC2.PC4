using LTC2.Shared.Messaging.Generic;
using LTC2.Shared.Messaging.Implementations.FileBasedBroker.Interfaces;
using LTC2.Shared.Messaging.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LTC2.Shared.Messaging.Implementations.FileBasedBroker
{
    public class Connection : AbstractBrokerConnection
    {
        private readonly ILogger<Connection> _logger;
        private readonly IFolderMonitorFactory _folderMonitorFactory;
        private readonly IServiceProvider _serviceProvider;

        private object _lockObject = new object();
        private IFolderMonitor _monitor;

        public Connection(ILogger<Connection> logger, string connectionString, IFolderMonitorFactory folderMonitorFactory, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _folderMonitorFactory = folderMonitorFactory;
            _serviceProvider = serviceProvider;

            ConnectionString = connectionString;
        }

        public string Folder
        {
            get
            {
                var connectionArguments = ConnectionString.Split(';');
                var folderArgument = connectionArguments.FirstOrDefault(s => s.ToLower().StartsWith("folder="));

                if (folderArgument != null)
                {
                    var argumentParts = folderArgument.Split("=");

                    if (argumentParts.Length > 1)
                    {
                        var folder = argumentParts[1];

                        if (!string.IsNullOrEmpty(folder))
                        {
                            return folder;
                        }
                    }
                }

                return null;
            }
        }

        public override IProducer CreateProducer(ITarget target)
        {
            var logger = _serviceProvider.GetService<ILogger<Producer>>();

            return new Producer(logger)
            {
                Connection = this,
                Target = target
            };
        }

        protected override IConsumer CreateConsumer(string id, ITarget target, Func<IMessage, Task> onMessage)
        {
            lock (_lockObject)
            {
                if (_monitor == null)
                {
                    _monitor = _folderMonitorFactory.CreateFolderMonitor(Folder);

                    _monitor.Start();
                }
            }

            var result = new Consumer()
            {
                Connection = this,
                Target = target,
                Callback = onMessage
            };

            _monitor.AddTarget(result);

            return result;
        }

        public override void RemoveConsumer(IConsumer consumer)
        {
            base.RemoveConsumer(consumer);

            lock (_lockObject)
            {
                if (_consumers.Count == 0)
                {
                    _monitor.Stop();

                    _monitor = null;
                }
            }
        }

        public override void Close()
        {

            base.Close();

            lock (_lockObject)
            {
                _monitor?.Stop();
            }
        }

        public void AcknowledgeMessage(Message message)
        {
            _monitor.Acknowledge(message);
        }
    }
}
