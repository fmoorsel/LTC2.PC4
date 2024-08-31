using LTC2.Shared.Messaging.Interfaces;
using System;
using System.Threading.Tasks;

namespace LTC2.Shared.Messaging.Implementations.FileBasedBroker
{
    public class Consumer : IConsumer
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public IBrokerConnection Connection { get; set; }

        public ITarget Target { get; set; }

        public bool IsOpen { get; set; }

        public Func<Message, Task> Callback { get; set; }

        public void Close()
        {
            IsOpen = false;

            Connection.RemoveConsumer(this);
        }
    }
}
