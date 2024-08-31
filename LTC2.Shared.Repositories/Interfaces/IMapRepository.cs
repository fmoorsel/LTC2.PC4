using LTC2.Shared.Models.Domain;
using System.Collections.Generic;

namespace LTC2.Shared.Repositories.Interfaces
{
    public interface IMapRepository : IBaseRepository
    {
        public bool HasMap();

        public void CreateAndPopulateMapIndex(bool forceReplace = false);

        public string InsertActivity(Activity activity);

        public List<Place> CheckTrack(List<List<double>> track);

        public List<Place> PreCheckTrack(List<List<double>> track);

        public List<Place> GetAllPlaces();

        public void CheckPreparedMap();

    }
}
