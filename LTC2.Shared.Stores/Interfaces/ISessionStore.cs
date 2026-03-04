using LTC2.Shared.Models.Domain;
using System.Threading.Tasks;

namespace LTC2.Shared.Stores.Interfaces
{
    public interface ISessionStore
    {
        public void Store(Session session);

        public Session Retrieve(long athleteId, string origin, Session currentSession = null);

        public Task<Session> RetrieveAsync(long athleteId, string origin, Session currentSession = null);
    }
}
