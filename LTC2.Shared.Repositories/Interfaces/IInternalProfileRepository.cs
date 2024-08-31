using LTC2.Shared.Models.Domain;
using System.Threading.Tasks;

namespace LTC2.Shared.Repositories.Interfaces
{
    public interface IInternalProfileRepository
    {
        public Task<InternalProfile> GetInternalProfile(long athleteId);

        public Task UpsertInternalProfile(InternalProfile profile);
    }
}
