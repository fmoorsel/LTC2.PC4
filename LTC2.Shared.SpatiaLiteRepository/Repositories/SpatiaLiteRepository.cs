using LTC2.Shared.Models.Domain;
using LTC2.Shared.Models.Dtos.SqlServer;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.SpatiaLiteRepository.RowMappers;
using LTC2.Shared.SpatiaLiteRepository.Utils;
using LTC2.Shared.Utils.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace LTC2.Shared.SpatiaLiteRepository.Repositories
{
    public class SpatiaLiteRepository : AbstractSqliteRepository
    {
        private readonly SpatiaLiteMapperSettings _spatiaLiteMapperSettings;
        private readonly GenericSettings _genericSettings;
        private readonly ILogger<SpatiaLiteRepository> _logger;

        private readonly string _queryHasMap = @"
            SELECT 
                name 
            FROM 
                sqlite_master 
            WHERE 
                type='table' AND name=@name;            
        ";

        private readonly string _queryCreateMapTable = @"
            CREATE TABLE Map
            (
                mapId INTEGER PRIMARY KEY AUTOINCREMENT,
                mapMapName VARCHAR(80),
                mapName VARCHAR(80),
                mapFeaturePointer VARCHAR(10)
            );
        ";

        private readonly string _queryAddMapBorderColumn = @"
            SELECT AddGeometryColumn(
                'Map', 
                'mapBorder',
                4326, 
                'POLYGON', 
                'XY'
            );
        ";

        private readonly string _queryAddMapHitAreaColumn = @"
            SELECT AddGeometryColumn(
                'Map', 
                'mapHitArea',
                4326, 
                'POLYGON', 
                'XY'
            );
        ";

        private readonly string _queryAddSpatialIndex = @"
            SELECT CreateSpatialIndex(
                'Map', 
                @MapColumn
            );
        ";

        private readonly string _queryInsertPlace = @"
            INSERT INTO Map 
            (
                mapMapName, 
                mapName,
                mapFeaturePointer, 
                mapBorder, 
                mapHitArea
            ) 
            VALUES 
            (
                @MapMapName, 
                @MapName, 
                @MapFeaturePointer, 
                ST_GeomFromText(@MapBorder, 4326), 
                ST_GeomFromText(@MapHitArea, 4326)
            );
        ";

        private readonly string _queryInitSpatialMetaData = @"
            SELECT InitSpatialMetaData('WGS84');
        ";


        private readonly string _queryPreCheckTrack = @"
            SELECT 
                mapId,
                mapName,
                mapFeaturePointer
            FROM 
                Map 
            WHERE 
                ST_Intersects(mapHitArea, ST_GeomFromText(@CurrTrack, 4326)) = 1
            AND
                ROWID in (
                    SELECT 
                        ROWID
                    FROM
                        SpatialIndex
                    WHERE
                        f_table_name = 'Map'
                    AND
                        f_geometry_column  = 'mapHitArea'
                    AND
                        search_frame = ST_GeomFromText(@CurrTrack, 4326)
                );            
        ";


        private readonly string _queryCheckTrack = @"
            SELECT 
                mapId,
                mapName,
                mapFeaturePointer
            FROM 
                Map 
            WHERE 
                ST_Intersects(mapBorder, ST_GeomFromText(@CurrTrack, 4326)) = 1
            AND
                ROWID in (
                    SELECT 
                        ROWID
                    FROM
                        SpatialIndex
                    WHERE
                        f_table_name = 'Map'
                    AND
                        f_geometry_column  = 'mapBorder'
                    AND
                        search_frame = ST_GeomFromText(@CurrTrack, 4326)
                );            
        ";

        private readonly string _queryGetAllPlaces = @"
            SELECT
                mapId,
                mapName,
                mapFeaturePointer
            FROM
                Map;            
        ";

        public SpatiaLiteRepository(
            ILogger<SpatiaLiteRepository> logger,
            GenericSettings genericSettings,
            SpatiaLiteMapperSettings spatiaLiteMapperSettings) : base()
        {
            _spatiaLiteMapperSettings = spatiaLiteMapperSettings;
            _genericSettings = genericSettings;
            _logger = logger;
        }

        public void CheckPreparedMap()
        {
            if (!string.IsNullOrEmpty(_spatiaLiteMapperSettings.PreparedMap))
            {
                try
                {
                    var preparedMap = _spatiaLiteMapperSettings.PreparedMap.Replace("@@MAPNAME@@", _genericSettings.Id);
                    var connectionString = _spatiaLiteMapperSettings.ConnectionString.Replace("@@MAPNAME@@", _genericSettings.Id);
                    var spatialiteFile = GetFileNameFromConnectionString(connectionString);

                    if (!File.Exists(spatialiteFile))
                    {
                        var folder = Path.GetDirectoryName(spatialiteFile);

                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }

                        File.Copy(preparedMap, spatialiteFile);
                    }

                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, $"Unable to use prepared spatialite database: {e.Message} (compiling new one is tried later)");
                }
            }
        }

        public override void Open()
        {
            var connectionString = _spatiaLiteMapperSettings.ConnectionString.Replace("@@MAPNAME@@", _genericSettings.Id);

            _connection = SpatiaLiteUtils.CreateSpatiaLiteConnection(connectionString);
        }

        private string GetFileNameFromConnectionString(string connectionString)
        {
            var result = string.Empty;
            var connStr = connectionString.ToLower();
            var sourceToken = "data source=";

            var startPos = connStr.IndexOf(sourceToken);
            if (startPos >= 0)
            {
                startPos += sourceToken.Length;

                var fileName = connectionString.Substring(startPos);
                var endTokenPos = fileName.IndexOf(';');

                if (endTokenPos >= 0)
                {
                    fileName = fileName.Substring(0, endTokenPos);
                }

                result = fileName;
            }

            return result;
        }

        public bool HasMap()
        {
            if (_connection != null)
            {
                var parameters = new Dictionary<string, object>()
                {
                    { "@name", "Map" }
                };

                var tables = GetRecords<string>(_connection, _queryHasMap, new StringRowMappers(), parameters);

                return tables.Count > 0;
            }

            return false;
        }

        public void CreateEmtpyMapTable()
        {
            if (_connection != null)
            {
                var hasTable = HasMap();

                if (hasTable)
                {
                    DropMapTable();
                }

                InitSpatiaLiteMetaData();

                CreateMapTable();

                AddGeoColumnsToMapTable();

                AddSpatialIndices();
            }
        }

        public void InsertPlace(string name, string featurePointer, string border, string hitArea)
        {
            if (_connection != null)
            {
                var parameters = new Dictionary<string, object>()
                {
                    { "@MapMapName", _genericSettings.Id },
                    { "@MapName", name },
                    { "@MapFeaturePointer", featurePointer},
                    { "@MapBorder", border },
                    { "@MapHitArea", hitArea }
                };

                ExecuteNonQuery(_connection, _queryInsertPlace, parameters);
            }
        }

        public List<Place> GetAllPlaces()
        {
            var result = new List<Place>();

            if (_connection != null)
            {

                var mapRecords = GetRecords<DtoMap>(_connection, _queryGetAllPlaces, new DtoMapRowMapper());

                foreach (var map in mapRecords)
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
            }

            return result;
        }

        private void AddSpatialIndices()
        {
            AddSpatialIndex("mapBorder");
            AddSpatialIndex("mapHitArea");
        }

        private void AddSpatialIndex(string column)
        {
            if (_connection != null)
            {
                var parameters = new Dictionary<string, object>()
                {
                    { "@MapColumn", column }
                };

                ExecuteNonQuery(_connection, _queryAddSpatialIndex, parameters);
            }
        }

        public List<Place> CheckTrack(List<List<double>> track)
        {
            return CheckTrack(_queryCheckTrack, track);
        }

        public List<Place> PreCheckTrack(List<List<double>> track)
        {
            return CheckTrack(_queryPreCheckTrack, track);
        }

        private List<Place> CheckTrack(string query, List<List<double>> track)
        {
            var result = new List<Place>();

            if (_connection != null)
            {
                var trackAsWkt = GeometryProducer.Instance.CreateLinestringAsWktString(track);
                var parameters = new Dictionary<string, object>()
                {
                    { "@CurrTrack", trackAsWkt }
                };

                var mapRecords = GetRecords<DtoMap>(_connection, query, new DtoMapRowMapper(), parameters);

                foreach (var map in mapRecords)
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
            }

            return result;
        }

        private void InitSpatiaLiteMetaData()
        {
            if (_connection != null)
            {
                ExecuteNonQuery(_connection, _queryInitSpatialMetaData);
            }
        }

        private void DropMapTable()
        {
            if (_connection != null)
            {
                _connection.Close();

                SqliteConnection.ClearAllPools();
            }

            var connectionString = _spatiaLiteMapperSettings.ConnectionString.Replace("@@MAPNAME@@", _genericSettings.Id);
            var fileName = GetFileNameFromConnectionString(connectionString);

            if (File.Exists(fileName))
            {
                var retryCount = 0;

                while (true)
                {
                    try
                    {
                        File.Delete(fileName);

                        break;
                    }
                    catch
                    {
                        Thread.Sleep(500);

                        retryCount++;

                        if (retryCount > 3)
                        {
                            throw;
                        }
                    }

                }
            }

            _connection = SpatiaLiteUtils.CreateSpatiaLiteConnection(connectionString);

        }

        private void CreateMapTable()
        {
            if (_connection != null)
            {
                var hasTable = HasMap();

                if (!hasTable)
                {
                    ExecuteNonQuery(_connection, _queryCreateMapTable);
                }
            }
        }

        private void AddGeoColumnsToMapTable()
        {
            if (_connection != null)
            {
                var hasTable = HasMap();

                if (hasTable)
                {
                    ExecuteNonQuery(_connection, _queryAddMapBorderColumn);

                    ExecuteNonQuery(_connection, _queryAddMapHitAreaColumn);
                }
            }
        }
    }
}
