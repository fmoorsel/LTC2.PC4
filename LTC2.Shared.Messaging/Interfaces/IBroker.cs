namespace LTC2.Shared.Messaging.Interfaces
{
    public interface IBroker<TMessage> where TMessage : IMessage
    {
        public IBrokerConnection Connect(string connectionString);

        public IMessage CreateEmptyMessage(MessageType type);

        public void Disconnect();

    }
}
