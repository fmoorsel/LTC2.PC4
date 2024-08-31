using LTC2.Shared.Models.Domain;
using LTC2.Shared.Models.Dtos.Memory;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.Repositories.Interfaces;
using LTC2.Shared.Utils.Utils;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using System.Collections.Generic;
using System.Linq;

namespace LTC2.Services.Calculator.Repositories
{
    public class MemoryMapRepository : IMapRepository
    {
        private readonly ILogger<SqlServerMapRepository> _logger;
        private readonly GenericSettings _genericSettings;
        private readonly IPlacesRepository _placesRepository;

        private STRtree<PlacePolygon> _spatialIndexPlaces;
        private STRtree<PlacePolygon> _spatialIndexHitArea;

        private List<Place> _placesCache;

        public MemoryMapRepository(ILogger<SqlServerMapRepository> logger, GenericSettings genericSettings, IPlacesRepository placesRepository)
        {
            _logger = logger;
            _genericSettings = genericSettings;
            _placesRepository = placesRepository;
        }

        public void CheckPreparedMap()
        {
        }

        public List<Place> CheckTrack(List<List<double>> track)
        {
            return CheckTrack(track, _spatialIndexPlaces);
        }

        public void Close()
        {
        }

        public void CreateAndPopulateMapIndex(bool forceReplace = false)
        {
            var placesIndex = new STRtree<PlacePolygon>();
            var hitAreaIndex = new STRtree<PlacePolygon>();
            var placesCache = new List<Place>();

            var places = _placesRepository.GetPlacesFromCache();

            if (forceReplace || places.Count == 0)
            {
                places = _placesRepository.GetPlaces();
                var neighbours = _placesRepository.GetNeighbourDefinitions();

                foreach (var place in places)
                {
                    _placesRepository.FillHitAreaFieldsForPlace(place, places, neighbours);
                }

                _placesRepository.CachePlaces(places);
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
                    var hitArea = place.HitArea;

                    var newPlace = new Place()
                    {
                        Id = id,
                        Name = name,
                        FeaturePointer = featurePointer
                    };

                    placesCache.Add(newPlace);

                    var indexPlace = new PlacePolygon(border.StandardizedizedPolygon.Shell, border.StandardizedizedPolygon.Holes, newPlace);
                    var indexEnvelope = new Envelope(indexPlace.Envelope.Coordinates);

                    var hitAreaPlace = new PlacePolygon(hitArea.Shell, hitArea.Holes, newPlace);
                    var hitAreaEnvelope = new Envelope(hitAreaPlace.Envelope.Coordinates);

                    hitAreaIndex.Insert(hitAreaEnvelope, hitAreaPlace);
                    placesIndex.Insert(indexEnvelope, indexPlace);

                    featureSequence++;
                }

                sequence++;
            }

            _spatialIndexHitArea = hitAreaIndex;
            _spatialIndexPlaces = placesIndex;
            _placesCache = placesCache;
        }

        public bool HasMap()
        {
            return _spatialIndexPlaces != null && _spatialIndexHitArea != null && _placesCache != null;
        }

        public string InsertActivity(Activity activity)
        {
            return string.Empty;
        }

        public void Open()
        {
        }

        public List<Place> PreCheckTrack(List<List<double>> track)
        {
            return CheckTrack(track, _spatialIndexHitArea);
        }

        public List<Place> CheckTrack(List<List<double>> track, STRtree<PlacePolygon> spatialIndex)
        {
            var result = new List<Place>();

            var lineString = GeometryProducer.Instance.CreateLinestring(track);
            var envelope = new Envelope(lineString.Envelope.Coordinates);

            var candidates = spatialIndex.Query(envelope);

            foreach (var candidate in candidates)
            {
                if (lineString.Intersects(candidate))
                {
                    if (result.FirstOrDefault(p => p.Id == candidate.Place.Id) == null)
                    {
                        result.Add(candidate.Place);
                    }
                }
            }

            return result;
        }

        public List<Place> GetAllPlaces()
        {
            return new List<Place>(_placesCache);
        }
    }
}
