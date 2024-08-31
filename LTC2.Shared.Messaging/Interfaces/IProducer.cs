namespace LTC2.Shared.Messaging.Interfaces
{

    public interface IProducer
    {
        public string Id { get; set; }

        public void Produce(IMessage data);
    }
}
