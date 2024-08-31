namespace LTC2.Shared.Messaging.Interfaces
{
    public interface IBrokerConnectionFactory<TBrokerConnectionType> where TBrokerConnectionType : IBrokerConnection
    {
        public TBrokerConnectionType CreateBrokerConnection(string connectionString);
    }
}
