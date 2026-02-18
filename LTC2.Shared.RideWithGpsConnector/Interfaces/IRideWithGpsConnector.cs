using LTC2.Shared.Models.Domain;
using System.Threading.Tasks;

namespace LTC2.Shared.RideWithGpsConnector.Interfaces
{
    public interface IRideWithGpsConnector
    {
        Task<Session> GetSession(string code);
    }
}
