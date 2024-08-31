using LTC2.Services.Calculator.Repositories.RowMappers;
using LTC2.Shared.Database.Respositories;
using LTC2.Shared.Models.Domain;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.Repositories.Interfaces;
using LTC2.Shared.Utils.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LTC2.Services.Calculator.Repositories
{
    public class SqlServerMapRepository : AbstractSqlRepository, IMapRepository
    {
        private readonly ILogger<SqlServerMapRepository> _logger;
        private readonly GenericSettings _genericSettings;
        private readonly IPlacesRepository _placesRepository;

        public SqlServerMapRepository(ILogger<SqlServerMapRepository> logger, GenericSettings genericSettings, IPlacesRepository placesRepository)
        {
            _logger = logger;
            _genericSettings = genericSettings;
            _placesRepository = placesRepository;

            DbConnectionString = _genericSettings.DatabaseConnectionString;
        }

        public List<Place> CheckTrack(List<List<double>> track)
        {
            return CheckTrack(track, "spGetPlacesOfCurrentTrack");
        }

        public void CheckPreparedMap()
        {
        }

        public List<Place> CheckTrack(List<List<double>> track, string storedProc)
        {
            var result = new List<Place>();
            var trackAsWkt = GeometryProducer.Instance.CreateLinestringAsWktString(track);

            var dbTrackParameter = new DbParameter("@Track", trackAsWkt);

            var dbParameters = new List<DbParameter>()
            {
                dbTrackParameter
            };

            ExecuteStoredProcedureNonQuery("spInsertCurrentActivity", dbParameters);

            var dbMapNameParameter = new DbParameter("@MapMapName", _genericSettings.Id);

            dbParameters = new List<DbParameter>()
            {
                dbMapNameParameter
            };

            var queryResult = GetRecordsByStoredProcedure(storedProc, new DtoMapRowMapper(), dbParameters);

            foreach (var map in queryResult)
            {
                var place = new Place()
                {
                    Id = map.mapFeaturePointer.Split(':')[0],
                    Name = map.mapName,
                    FeaturePointer = map.mapFeaturePointer
                };

                if (result.FirstOrDefault(p => p.Id == place.Id) == null)
                {
                    result.Add(place);
                }
            }

            return result;
        }

        public void Close()
        {

        }

        public void CreateAndPopulateMapIndex(bool forceReplace = false)
        {
            EmptyMap();

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

                    InsertPlace(name, featurePointer, wktBorder, hitArea);

                    featureSequence++;
                }

                sequence++;
            }

        }

        private void EmptyMap()
        {
            var dbMapNameParameyer = new DbParameter("@MapMapName", _genericSettings.Id);

            var dbParameters = new List<DbParameter>()
            {
                dbMapNameParameyer
            };

            ExecuteStoredProcedureNonQuery("spInitPlaces", dbParameters);
        }

        private void InsertPlace(string name, string featurePointer, string border, string hitArea)
        {
            var dbMapNameParameter = new DbParameter("@MapMapName", _genericSettings.Id);
            var dbNameParameter = new DbParameter("@MapName", name);
            var dbFeaturePointer = new DbParameter("@MapFeaturePointer", featurePointer);
            var dbBorder = new DbParameter("@MapBorder", border);
            var dbHitArea = new DbParameter("@MapHitArea", hitArea);

            var dbParameters = new List<DbParameter>()
            {
                dbMapNameParameter, dbNameParameter, dbFeaturePointer, dbBorder, dbHitArea
            };

            ExecuteStoredProcedureNonQuery("spAddPlace", dbParameters);
        }

        public bool HasMap()
        {
            var dbMapNameParameyer = new DbParameter("@MapMapName", _genericSettings.Id);

            var dbParameters = new List<DbParameter>()
            {
                dbMapNameParameyer
            };

            var queryResult = GetRecordsByStoredProcedure("spGetPlacesCount", new DtoMapCountRowmapper(), dbParameters);

            return (queryResult.Count > 0) && (queryResult[0].mapCount > 0);
        }

        public string InsertActivity(Activity activity)
        {
            var trackAsWkt = GeometryProducer.Instance.CreateLinestringAsWktString(activity.Track);

            var dbActiExternalId = new DbParameter("@ActiExternalId", activity.Id);
            var dbNameParameter = new DbParameter("@ActiName", activity.Name);
            var dbAthleteIdr = new DbParameter("@ActiAthleteId", Convert.ToInt64(activity.AthleteId));
            var dbTrack = new DbParameter("@ActiTrack", trackAsWkt);


            var dbParameters = new List<DbParameter>()
            {
                dbActiExternalId, dbNameParameter, dbAthleteIdr, dbTrack
            };

            var queryResult = GetRecordsByStoredProcedure("spInsertActivity", new DtoActivityIdRowMapper(), dbParameters);

            return (queryResult.Count > 0) ? queryResult[0].actiId.ToString() : String.Empty;
        }

        public void Open()
        {

        }

        public List<Place> PreCheckTrack(List<List<double>> track)
        {
            return CheckTrack(track, "spGetPotentialPlacesOfCurrentTrack");
        }

        public List<Place> GetAllPlaces()
        {
            var result = new List<Place>();

            var dbMapNameParameyer = new DbParameter("@MapMapName", _genericSettings.Id);

            var dbParameters = new List<DbParameter>()
            {
                dbMapNameParameyer
            };

            var queryResult = GetRecordsByStoredProcedure("spGetAllPlaces", new DtoMapRowMapper(), dbParameters);

            foreach (var map in queryResult)
            {
                var place = new Place()
                {
                    Id = map.mapId.ToString(),
                    Name = map.mapName,
                    FeaturePointer = map.mapFeaturePointer
                };

                result.Add(place);
            }

            return result;
        }
    }
}
