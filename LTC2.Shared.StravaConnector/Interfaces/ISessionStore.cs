using LTC2.Shared.Models.Domain;
using System.Threading.Tasks;

namespace LTC2.Shared.StravaConnector.Interfaces
{
    public interface ISessionStore
    {
        public void Store(Session session);

        public Session Retrieve(long athleteId, Session currentSession = null);

        public Task<Session> RetrieveAsync(long athleteId, Session currentSession = null);
    }
}
