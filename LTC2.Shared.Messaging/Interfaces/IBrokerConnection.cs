using System;
using System.Threading.Tasks;

namespace LTC2.Shared.Messaging.Interfaces
{
    public interface IBrokerConnection
    {
        public string ConnectionString { get; set; }

        public bool IsConnected { get; set; }

        public void Close();

        public IConsumer AddConsumer(ITarget target, Func<IMessage, Task> onMessage);

        public void RemoveConsumer(IConsumer consumer);

        public IProducer CreateProducer(ITarget target);
    }
}
