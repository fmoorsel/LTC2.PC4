using LTC2.Shared.Messaging.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace LTC2.Shared.Messaging.Generic
{
    public abstract class AbstractBrokerConnection : IBrokerConnection
    {

        protected readonly ConcurrentDictionary<string, IConsumer> _consumers;

        public AbstractBrokerConnection()
        {
            _consumers = new ConcurrentDictionary<string, IConsumer>();
        }

        public string ConnectionString { get; set; }

        public bool IsConnected { get; set; }

        public virtual IConsumer AddConsumer(ITarget target, Func<IMessage, Task> onMessage)
        {
            var id = Guid.NewGuid().ToString();
            var consumer = CreateConsumer(id, target, onMessage);

            consumer.IsOpen = true;

            if (consumer.Id == null)
            {
                consumer.Id = id;
            }

            if (consumer.Connection == null)
            {
                consumer.Connection = this;
            }

            _consumers.TryAdd(id, consumer);

            return consumer;
        }

        protected abstract IConsumer CreateConsumer(string id, ITarget target, Func<IMessage, Task> onMessage);

        public virtual void Close()
        {
            if (IsConnected)
            {
                var consumers = _consumers.Values;

                foreach (var consumer in consumers)
                {
                    RemoveConsumer(consumer);
                }
            }

            IsConnected = false;
        }

        public abstract IProducer CreateProducer(ITarget target);

        public virtual void RemoveConsumer(IConsumer consumer)
        {
            if (consumer.IsOpen)
            {
                consumer.IsOpen = false;

                consumer.Close();
            }

            _consumers.TryRemove(consumer.Id, out IConsumer removedConsumer);
        }

    }
}
