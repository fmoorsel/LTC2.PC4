using LTC2.Shared.Messaging.Exceptions;
using LTC2.Shared.Messaging.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace LTC2.Shared.Messaging.Implementations.FileBasedBroker
{
    public class Broker : IBroker<Message>
    {
        private readonly ILogger _logger;

        private Connection _connection;

        private readonly IBrokerConnectionFactory<Connection> _brokerConnectionFactory;
        private readonly object _lockObject = new object();

        public Broker(ILogger<Broker> logger, IBrokerConnectionFactory<Connection> brokerConnectionFactory)
        {
            _brokerConnectionFactory = brokerConnectionFactory;
            _logger = logger;
        }

        public IBrokerConnection Connect(string connectionString)
        {
            lock (_lockObject)
            {
                if (_connection == null)
                {
                    _connection = _brokerConnectionFactory.CreateBrokerConnection(connectionString);

                    if (_connection.Folder != null)
                    {
                        if (!Directory.Exists(_connection.Folder))
                        {
                            try
                            {
                                Directory.CreateDirectory(_connection.Folder);
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, $"Unable to create folder {_connection.Folder} from connectionstring {connectionString}");

                                throw new BadConnectionStringException();
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError($"No or incomplete folder argument found in connectionstring {connectionString}");

                        throw new BadConnectionStringException();
                    }

                    _connection.IsConnected = true;
                }

                return _connection;

            }
        }

        public void Disconnect()
        {
            lock (_lockObject)
            {
                _connection.Close();

                _connection = null;
            }
        }

        public IMessage CreateEmptyMessage(MessageType type)
        {
            return new Message();
        }
    }
}
