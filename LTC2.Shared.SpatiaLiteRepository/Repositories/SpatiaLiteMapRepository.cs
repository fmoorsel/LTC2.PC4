using LTC2.Shared.Models.Domain;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace LTC2.Shared.SpatiaLiteRepository.Repositories
{
    public class SpatiaLiteMapRepository : IMapRepository
    {
        private readonly SpatiaLiteRepository _spatiaLiteRepository;
        private readonly ILogger<SpatiaLiteMapRepository> _logger;
        private readonly GenericSettings _genericSettings;
        private readonly IPlacesRepository _placesRepository;

        public SpatiaLiteMapRepository(
            ILogger<SpatiaLiteMapRepository> logger,
            GenericSettings genericSettings,
            IPlacesRepository placesRepository,
            SpatiaLiteRepository spatiaLiteRepository)
        {
            _logger = logger;
            _genericSettings = genericSettings;
            _placesRepository = placesRepository;
            _spatiaLiteRepository = spatiaLiteRepository;
        }

        public void CheckPreparedMap()
        {
            _spatiaLiteRepository.CheckPreparedMap();
        }

        public List<Place> CheckTrack(List<List<double>> track)
        {
            return _spatiaLiteRepository.CheckTrack(track);
        }

        public void Close()
        {
            _spatiaLiteRepository.Close();
        }

        public void CreateAndPopulateMapIndex(bool forceReplace = false)
        {
            _spatiaLiteRepository.CreateEmtpyMapTable();

            var places = _placesRepository.GetPlaces();
            var neighbours = _placesRepository.GetNeighbourDefinitions();

            foreach (var place in places)
            {
                _placesRepository.FillHitAreaFieldsForPlace(place, places, neighbours);
            }

            var sequence = 1;

            foreach (var place in places)
            {
                var id = place.ID;
                var name = place.Name;

                var featureSequence = 1;

                foreach (var border in place.Borders)
                {
                    var featurePointer = $"{id}:{featureSequence:D4}";

                    var wktBorder = border.Wkt;
                    var hitArea = place.HitAreaAsWkt;

                    _spatiaLiteRepository.InsertPlace(name, featurePointer, wktBorder, hitArea);

                    featureSequence++;
                }

                sequence++;
            }
        }

        public List<Place> GetAllPlaces()
        {
            return _spatiaLiteRepository.GetAllPlaces();
        }

        public bool HasMap()
        {
            return _spatiaLiteRepository.HasMap();

        }

        public string InsertActivity(Activity activity)
        {
            return string.Empty;
        }

        public void Open()
        {
            _spatiaLiteRepository.Open();
        }

        public List<Place> PreCheckTrack(List<List<double>> track)
        {
            return _spatiaLiteRepository.PreCheckTrack(track);
        }
    }
}
