namespace LTC2.Shared.Messaging.Interfaces
{
    public interface IBrokerFactory<TMessage> where TMessage : IMessage
    {
        public IBroker<TMessage> CreateBroker();
    }
}
