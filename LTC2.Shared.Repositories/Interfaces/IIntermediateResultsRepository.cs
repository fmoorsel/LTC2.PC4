using LTC2.Shared.Models.Domain;

namespace LTC2.Shared.Repositories.Interfaces
{
    public interface IIntermediateResultsRepository : IBaseRepository
    {
        public void StoreIntermedidateResult(CalculationResult calculationResult);

        public void Clear(long athleteId);

        public bool HasIntermediateResult(long athleteId);

        public CalculationResult GetIntermediateResult(long athleteId);

    }
}
