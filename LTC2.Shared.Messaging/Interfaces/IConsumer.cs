using LTC2.Shared.Messaging.Implementations.FileBasedBroker;
using System;
using System.Threading.Tasks;

namespace LTC2.Shared.Messaging.Interfaces
{
    public interface IConsumer
    {
        public string Id { get; set; }

        public Func<Message, Task> Callback { get; set; }

        public IBrokerConnection Connection { get; set; }

        public ITarget Target { get; set; }

        public bool IsOpen { get; set; }

        public void Close();


    }
}
