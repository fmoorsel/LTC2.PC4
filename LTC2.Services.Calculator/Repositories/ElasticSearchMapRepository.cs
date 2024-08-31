using LTC2.Services.Calculator.Models;
using LTC2.Shared.Models.Domain;
using LTC2.Shared.Models.Dtos.Elastic;
using LTC2.Shared.Models.Mapdefinitions;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.Repositories.Interfaces;
using LTC2.Shared.Repositories.Repositories;
using Microsoft.Extensions.Logging;
using Nest;
using System.Collections.Generic;
using System.Linq;

namespace LTC2.Services.Calculator.Repositories
{
    public class ElasticSearchMapRepository : AbstractElasticSearchRepository, IMapRepository
    {
        private readonly CalculatorSettings _settings;
        private readonly IPlacesRepository _placesRepository;

        private readonly string _tempId = "temp";

        public ElasticSearchMapRepository(ElasticUploaderSettings properties, ILogger<AbstractElasticSearchRepository> logger, CalculatorSettings settings, IPlacesRepository placesRepository) : base(properties, logger)
        {
            _settings = settings;
            _placesRepository = placesRepository;
        }

        public void CheckPreparedMap()
        {
        }

        public void CreateAndPopulateMapIndex(bool forceReplace = false)
        {
            if (forceReplace)
            {
                if (HasMap())
                {
                    _elasticSearchClient.DeleteIndex(_settings.MapName);
                }

                if (HasActivities())
                {
                    _elasticSearchClient.DeleteIndex(_settings.ActivitiesName);
                }
            }

            CreateIndex("map", _settings.MapName);
            CreateIndex("activities", _settings.ActivitiesName);

            var places = _placesRepository.GetPlaces();
            var neighbours = _placesRepository.GetNeighbourDefinitions();

            foreach (var place in places)
            {
                var hitArea = _placesRepository.GetHitAreaForPlace(place, places, neighbours);

                place.HitArea = hitArea;
            }

            var sequence = 1;

            var placeDtos = new List<PlaceDto>();

            foreach (var place in places)
            {
                var dtos = CreatePlaceDtos(place, sequence);

                placeDtos.AddRange(CreatePlaceDtos(place, sequence));

                sequence++;
            }

            _elasticSearchClient.BulkInsertDocuments<PlaceDto>(_settings.MapName, placeDtos);
        }

        public bool HasMap()
        {
            return _elasticSearchClient.IndexExists(_settings.MapName);
        }

        public bool HasActivities()
        {
            return _elasticSearchClient.IndexExists(_settings.ActivitiesName);
        }

        public string InsertActivity(Activity activity)
        {
            var activityDto = new ActivityDto()
            {
                ExternalId = activity.Id,
                AthleteId = activity.AthleteId,
                Name = activity.Name,
                Track = new TrackDto()
                {
                    Coordinates = activity.Track
                }

            };

            var id = _elasticSearchClient.InsertDocument<ActivityDto>(_settings.ActivitiesName, activityDto);

            return id;
        }

        public List<Place> PreCheckTrack(List<List<double>> track)
        {
            return CheckTrack(track, "hitArea");
        }

        public List<Place> CheckTrack(List<List<double>> track)
        {
            return CheckTrack(track, "border");
        }

        private List<Place> CheckTrack(List<List<double>> track, string borderField)
        {
            var result = new List<Place>();

            //            _elasticSearchClient.DeleteDocument<ActivityDto>(_settings.ActivitiesName, _tempId);

            var activityDto = new ActivityDto()
            {
                Id = _tempId,
                Track = new TrackDto()
                {
                    Coordinates = track
                }

            };

            //            var id = _elasticSearchClient.InsertDocument<ActivityDto>(_settings.ActivitiesName, activityDto);

            try
            {
                /*
                QueryContainer query(QueryContainerDescriptor<PlaceDto> exp) => exp
                    .Bool(b => b
                        .Filter(f => f
                            .GeoShape(q => q
                                .Field(borderField)
                                .IndexedShape(i => i
                                    .Index("nederland-towns-activities")
                                    .Path("track")
                                    .Id(_tempId)
                                )
                                .Relation(GeoShapeRelation.Intersects)
                            )
                        )
                    );
                */

                var coordinates = track.Select(t => new GeoCoordinate(t[1], t[0])).ToArray();

                QueryContainer query(QueryContainerDescriptor<PlaceDto> exp) => exp
                    .Bool(b => b
                        .Filter(f => f
                            .GeoShape(q => q
                                .Field(borderField)
                                .Shape(s =>
                                    s.LineString(coordinates)
                                )
                                .Relation(GeoShapeRelation.Intersects)
                            )
                        )
                    );
                var esResult = _elasticSearchClient.SearchDocument<PlaceDto>(_settings.MapName, query, _settings.MaxPlacesCount);

                foreach (var doc in esResult.Documents)
                {
                    var place = new Place()
                    {
                        Id = doc.PlaceId,
                        Name = doc.Name,
                        FeaturePointer = doc.FeaturePointer
                    };

                    if (result.FirstOrDefault(p => p.Id == place.Id) == null)
                    {
                        result.Add(place);
                    }
                }
            }
            finally
            {
                //               _elasticSearchClient.DeleteDocument<ActivityDto>(_settings.ActivitiesName, _tempId);
            }

            return result;
        }

        private List<PlaceDto> CreatePlaceDtos(PlaceDefination place, int sequence)
        {
            var placeDtos = new List<PlaceDto>();
            var featureSequence = 1;

            foreach (var border in place.BorderPolygons)
            {
                var placeDto = new PlaceDto();

                placeDto.PlaceId = place.ID;
                placeDto.Name = place.Name;
                placeDto.FeaturePointer = $"{placeDto.PlaceId}:{featureSequence:D4}";
                placeDto.Border = new BorderDto();
                placeDto.HitArea = new BorderDto();

                foreach (var pGon in border)
                {
                    var coordDtos = new List<List<double>>();

                    foreach (var coord in pGon.Coordinates)
                    {
                        var coordDto = new List<double>();

                        coordDto.Add(coord.X);
                        coordDto.Add(coord.Y);

                        coordDtos.Add(coordDto);
                    }

                    if (coordDtos.Count > 0)
                    {
                        if (coordDtos[0][0] != coordDtos[coordDtos.Count - 1][0] || coordDtos[0][1] != coordDtos[coordDtos.Count - 1][1])
                        {
                            coordDtos.Add(coordDtos[0]);
                        }

                    }

                    placeDto.Border.Coordinates.Add(coordDtos);
                }

                var hitAreaCoordDtos = new List<List<double>>();

                foreach (var coord in place.HitArea.Coordinates)
                {
                    var coordDto = new List<double>();

                    coordDto.Add(coord.X);
                    coordDto.Add(coord.Y);

                    hitAreaCoordDtos.Add(coordDto);
                }

                placeDto.HitArea.Coordinates.Add(hitAreaCoordDtos);

                placeDtos.Add(placeDto);

                featureSequence++;
            }

            return placeDtos;
        }

        public List<Place> GetAllPlaces()
        {
            var result = new List<Place>();

            QueryContainer query(QueryContainerDescriptor<PlaceDto> exp) => exp
                    .QueryString(q => q
                        .Query(string.Empty)
                        .DefaultField("id")
                        .DefaultOperator(Operator.And)
                    );


            var esResult = _elasticSearchClient.SearchDocument<PlaceDto>(_settings.MapName, query, _settings.MaxPlacesCount);

            foreach (var doc in esResult.Documents)
            {
                var place = new Place()
                {
                    Id = doc.PlaceId,
                    Name = doc.Name,
                    FeaturePointer = doc.FeaturePointer
                };

                result.Add(place);
            }


            return result;
        }
    }
}
