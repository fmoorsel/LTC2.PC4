using LTC2.Shared.Messaging.Interfaces;
using System;

namespace LTC2.Shared.Messaging.Implementations.FileBasedBroker
{
    public class Message : IMessage
    {
        public MessageType Type { get; set; } = MessageType.Text;

        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Group { get; set; }

        public object Payload { get; set; }

        public MessagePriority Priority { get; set; } = MessagePriority.Medium;

        public Consumer Consumer { get; set; }

        public ITarget Target { get; set; }

        public string FileName { get; set; }

        public Connection Connection { get; set; }

        public void Acknowlegde()
        {
            Connection.AcknowledgeMessage(this);
        }
    }
}
