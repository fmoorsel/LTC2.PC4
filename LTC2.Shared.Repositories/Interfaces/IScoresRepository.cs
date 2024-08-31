using LTC2.Shared.Models.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LTC2.Shared.Repositories.Interfaces
{
    public interface IScoresRepository
    {
        public void Open();

        public Task StoreScores(bool isRefresh, CalculationResult calculationResult);

        public Task<Visit> GetMostRecentVisit(long athleteId);

        public Task<CalculationResult> GetMostRecentResult(long athleteId);

        public Task<Dictionary<string, Track>> GetTracks(long athleteId);

        public Task<Track> GetAlltimeTrackForPlace(long athleteId, string placeId, bool detailed);

        public Task<List<Track>> GetAlltimeTracksForAllPlaces(long athleteId);

    }
}
