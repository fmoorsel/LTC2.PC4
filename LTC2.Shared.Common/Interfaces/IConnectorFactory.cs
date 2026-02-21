using LTC2.Shared.Models.Domain;

namespace LTC2.Shared.Common.Interfaces
{
    public interface IConnectorFactory
    {
        IConnector Create(ConnectorSource source);
    }
}
