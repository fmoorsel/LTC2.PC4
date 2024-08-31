using LTC2.Shared.Models.Domain;
using LTC2.Shared.Models.Dtos.SqlServer;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.Repositories.Interfaces;
using LTC2.Shared.StravaConnector.Models;
using LTC2.Shared.Utils.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace LTC2.Shared.SpatiaLiteRepository.Repositories
{
    public class SqliteScoreRepository : AbstractSqliteRepository, IScoresRepository
    {
        private readonly GenericSettings _genericSettings;
        private readonly ILogger _logger;

        private readonly string _queryClearScores = @"
            DELETE 
            FROM 
                AlltimeScores
            WHERE 
                alltAthleteId = @AthleteId  
            AND 
                alltMapName = @MapName;
        ";

        private readonly string _queryUpsertScores = @"
            INSERT INTO AlltimeScores 
            (
                alltMapName, 
                alltExternalId,
                alltAthleteId,
                alltDate,
                alltPlaceId
            ) 
            VALUES 
            (
                @MapName, 
                @ExternalId, 
                @AthleteId,
                @Date,
                @PlaceId
            )
            ON CONFLICT(alltMapName,alltAthleteId,alltPlaceId)
            DO UPDATE SET
                alltDate = excluded.alltDate,
                alltExternalId = excluded.alltExternalId;
        ";


        private readonly string _queryClearCurrentYearScores = @"
            DELETE 
            FROM 
                CurrentYearScores
            WHERE 
                currAthleteId = @AthleteId  
            AND 
                currMapName = @MapName;
        ";

        private readonly string _queryUpsertCurrentYearScores = @"
            INSERT INTO CurrentYearScores 
            (
                currMapName, 
                currExternalId,
                currAthleteId,
                currDate,
                currPlaceId
            ) 
            VALUES 
            (
                @MapName, 
                @ExternalId, 
                @AthleteId,
                @Date,
                @PlaceId
            )
            ON CONFLICT(currMapName,currAthleteId,currPlaceId)
            DO UPDATE SET
                currDate = excluded.currDate,
                currExternalId = excluded.currExternalId;
        ";

        private readonly string _queryClearLastRideScores = @"
            DELETE 
            FROM 
                CurrentLastRidesScores
            WHERE 
                lastAthleteId = @AthleteId  
            AND 
                lastMapName = @MapName;
        ";

        private readonly string _queryInsertLastRideScores = @"
            INSERT INTO CurrentLastRidesScores
            (
                lastMapName, 
                lastExternalId,
                lastAthleteId,
                lastDate,
                lastPlaceId
            ) 
            VALUES 
            (
                @MapName, 
                @ExternalId, 
                @AthleteId,
                @Date,
                @PlaceId
            );
        ";

        private readonly string _queryUpsertTracks = @"
            INSERT INTO Tracks 
            (
                tracExternalId,
                tracAthleteId,
                tracName,
                tracTrack,
                tracDate,
                tracDistance,
                tracPlaces
            ) 
            VALUES 
            (
                @ExternalId, 
                @AthleteId,
                @Name,
                @Track,
                @Date,
                @Distance,
                @Places
            )
            ON CONFLICT(tracExternalId)
            DO UPDATE SET
                tracExternalId = excluded.tracExternalId,
                tracAthleteId = excluded.tracAthleteId,
                tracName = excluded.tracName,
                tracTrack = excluded.tracTrack,
                tracDate = excluded.tracDate,
                tracDistance = excluded.tracDistance,
                tracPlaces = excluded.tracPlaces;
        ";


        private readonly string _querySelectAllTimeScores = @"
            SELECT
                alltId,
                alltMapName, 
                alltExternalId,
                alltAthleteId,
                alltDate,
                alltPlaceId
            FROM
                AlltimeScores
            WHERE 
                alltAthleteId = @AthleteId 
            AND 
                alltMapName = @MapName
            ORDER BY 
                alltDate ASC;
        ";

        private readonly string _querySelectCurrentYearScores = @"
            SELECT
                currId,
                currMapName, 
                currExternalId,
                currAthleteId,
                currDate,
                currPlaceId
            FROM
                CurrentYearScores
            WHERE 
                currAthleteId = @AthleteId 
            AND 
                currMapName = @MapName
            ORDER BY 
                currDate ASC;
        ";

        private readonly string _querySelectCurrentLastRideScores = @"
            SELECT
                lastId,
                lastMapName, 
                lastExternalId,
                lastAthleteId,
                lastDate,
                lastPlaceId
            FROM
                CurrentLastRidesScores
            WHERE 
                lastAthleteId = @AthleteId 
            AND 
                lastMapName = @MapName
            ORDER BY 
                lastDate ASC;
        ";

        private readonly string _querySelectTracks = @"
            SELECT
                tracId,
                tracExternalId,
                tracName,
                tracAthleteId,
                tracDate,
                tracDistance,
                tracTrack,
                tracPlaces
            FROM
                Tracks
            WHERE 
                tracAthleteId = @AthleteId 
            ORDER BY 
                tracDate ASC;
        ";

        private readonly string _querySelectAlltimeTracksForAllPlaces = @"
            SELECT 
                tracId,
                tracExternalId,
                tracAthleteId,
                tracName,
                tracTrack,
                tracDate,
                tracDistance,
                tracPlaces	
            FROM
                Tracks
            WHERE EXISTS
                (
                    SELECT
                        DISTINCT alltExternalId
                    FROM 
                        AlltimeScores
                    WHERE 
                        alltMapName = @MapName AND alltAthleteId = @AthleteId AND alltExternalId = tracExternalId
                )       
            ORDER BY tracDate ASC;
        ";

        private readonly string _querySelectAlltimeTracksForPlace = @"
            SELECT 
                alltId,
                alltPlaceId,
                tracId,
                tracExternalId,
                tracAthleteId,
                tracName,
                tracTrack,
                tracDate,
                tracDistance,
                tracPlaces
            FROM 
                AlltimeScores
            INNER JOIN 
                Tracks
            ON alltExternalId = tracExternalId
            WHERE 
                alltMapName = @MapName AND alltPlaceId=@Place AND alltAthleteId = @AthleteId
            LIMIT 1
        ";

        private readonly string _querySelectMostRecentVisit = @"
            SELECT
                lastId,
                lastMapName, 
                lastExternalId,
                lastAthleteId,
                lastDate,
                lastPlaceId
            FROM
                CurrentLastRidesScores
            WHERE 
                lastAthleteId = @AthleteId AND lastMapName = @MapName
            LIMIT 1
        ";

        public SqliteScoreRepository(ILogger<SqliteScoreRepository> logger, GenericSettings genericSettings) : base()
        {
            _genericSettings = genericSettings;
            _logger = logger;

            DbConnectionString = _genericSettings.DatabaseConnectionSqliteString;
        }

        public async Task<Track> GetAlltimeTrackForPlace(long athleteId, string placeId, bool detailed)
        {
            using (var connection = new SqliteConnection(DbConnectionString))
            {
                await connection.OpenAsync();

                try
                {
                    var parameters = new Dictionary<string, object>()
                    {
                        { "@AthleteId", athleteId },
                        { "@MapName", _genericSettings.Id },
                        { "@Place", placeId }
                    };

                    var queryResult = await GetRecordsAsync<DtoTrackForAlltimePlace>(connection, _querySelectAlltimeTracksForPlace, new RowMappers.DtoTrackForAlltimePlaceRowMappers(), parameters);

                    if (queryResult.Count > 0)
                    {
                        var track = new Track()
                        {
                            ExternalId = queryResult[0].tracExternalId,
                            Name = queryResult[0].tracName,
                            Distance = queryResult[0].tracDistance,
                            VisitedOn = queryResult[0].tracDate,
                            Coordinates = GeometryProducer.Instance.GetTrackFromWktLineString(queryResult[0].tracTrack),
                            Places = string.IsNullOrEmpty(queryResult[0].tracPlaces) ? new List<string>() : queryResult[0].tracPlaces.Split(',').ToList()
                        };

                        if (detailed)
                        {
                            var detailedTrack = TryGetDetailedTrack(athleteId, queryResult[0].tracExternalId);
                            track.Coordinates = new List<List<double>>();

                            if (detailedTrack.Count > 1)
                            {
                                foreach (var coordinate in detailedTrack)
                                {
                                    var newCoordinate = new List<double>() { coordinate[1], coordinate[0] };
                                    track.Coordinates.Add(newCoordinate);
                                }
                            }
                        }

                        return track;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Unable to get Tracks due to: {ex.Message}");

                    throw;
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }

            return null;
        }

        private List<List<double>> TryGetDetailedTrack(long athleteId, string activityId)
        {
            var result = new List<List<double>>();

            try
            {
                return GetStreamFromCaches(athleteId, activityId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Unable to get detailed track due to: {ex.Message}");

            }

            return result;
        }

        private List<List<double>> GetStreamFromCaches(long athleteId, string activityId)
        {
            var result = GetStreamFromCache(athleteId, activityId, false);

            if (result.Count > 1)
            {
                return result;
            }

            return GetStreamFromCache(athleteId, activityId, true);
        }

        private List<List<double>> GetStreamFromCache(long athleteId, string activityId, bool archiveCache)
        {
            var cacheFolder = Path.Combine(_genericSettings.CacheFolder, "Streams");

            if (archiveCache)
            {
                cacheFolder = Path.Combine(cacheFolder, $"{athleteId}");
            }

            var fileName = Path.Combine(cacheFolder, $"r{activityId}");

            if (File.Exists(fileName))
            {
                try
                {
                    var json = File.ReadAllText(fileName);
                    var cachedResult = JsonConvert.DeserializeObject<StravaActivityCoordinateStream>(json);

                    var result = new List<List<double>>();

                    foreach (var coordinate in cachedResult.Latlng.Data)
                    {
                        result.Add(coordinate);
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    var archiveToken = archiveCache ? "archive" : "";

                    _logger.LogWarning(ex, $"Unable to read activity {activityId} in {fileName} from {archiveToken}cache due to {ex.Message}");
                }
            }

            return new List<List<double>>();
        }

        public async Task<List<Track>> GetAlltimeTracksForAllPlaces(long athleteId)
        {
            var result = new List<Track>();

            using (var connection = new SqliteConnection(DbConnectionString))
            {
                await connection.OpenAsync();

                try
                {
                    var parameters = new Dictionary<string, object>()
                    {
                        { "@AthleteId", athleteId },
                        { "@MapName", _genericSettings.Id }
                    };

                    var queryResult = await GetRecordsAsync<DtoTrack>(connection, _querySelectAlltimeTracksForAllPlaces, new RowMappers.DtoTrackRowMapper(), parameters);

                    foreach (var trackDto in queryResult)
                    {
                        var track = new Track()
                        {
                            ExternalId = trackDto.tracExternalId,
                            Name = trackDto.tracName,
                            Distance = trackDto.tracDistance,
                            VisitedOn = trackDto.tracDate,
                            Coordinates = GeometryProducer.Instance.GetTrackFromWktLineString(trackDto.tracTrack),
                            Places = string.IsNullOrEmpty(trackDto.tracPlaces) ? new List<string>() : trackDto.tracPlaces.Split(',').ToList()
                        };

                        result.Add(track);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Unable to get Tracks due to: {ex.Message}");

                    throw;
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }

            return result;
        }

        public async Task<CalculationResult> GetMostRecentResult(long athleteId)
        {
            var result = new CalculationResult()
            {
                AthleteId = athleteId
            };

            using (var connection = new SqliteConnection(DbConnectionString))
            {
                await connection.OpenAsync();

                try
                {
                    var athleteTracks = await GetTracks(athleteId);

                    var parameters = new Dictionary<string, object>()
                    {
                        { "@AthleteId", athleteId },
                        { "@MapName", _genericSettings.Id }
                    };

                    var queryResultAlltime = await GetRecordsAsync<DtoAllTimeScore>(connection, _querySelectAllTimeScores, new RowMappers.DtoAllTimeScoreRowMapper(), parameters);

                    if (queryResultAlltime.Count > 0)
                    {
                        foreach (var visitDto in queryResultAlltime)
                        {
                            if (athleteTracks.ContainsKey(visitDto.alltExternalId))
                            {
                                var track = athleteTracks[visitDto.alltExternalId];

                                var visit = new Visit()
                                {
                                    PlaceId = visitDto.alltPlaceId,
                                    ExternalId = visitDto.alltExternalId,
                                    VisitedOn = visitDto.alltDate,
                                    Name = track.Name,
                                    Distance = track.Distance,
                                    Track = track.Coordinates
                                };

                                result.VisitedPlacesAllTime.Add(visit.PlaceId, visit);

                            }
                        }

                        var queryResultCurrentYear = await GetRecordsAsync<DtoCurrentYearScore>(connection, _querySelectCurrentYearScores, new RowMappers.DtoCurrentYearScoreRowMapper(), parameters);

                        foreach (var visitDto in queryResultCurrentYear)
                        {
                            if (visitDto.currDate.Year == DateTime.UtcNow.Year && athleteTracks.ContainsKey(visitDto.currExternalId))
                            {
                                var track = athleteTracks[visitDto.currExternalId];

                                var visit = new Visit()
                                {
                                    PlaceId = visitDto.currPlaceId,
                                    ExternalId = visitDto.currExternalId,
                                    VisitedOn = visitDto.currDate,
                                    Name = track.Name,
                                    Distance = track.Distance,
                                    Track = track.Coordinates
                                };


                                result.VisitedPlacesCurrentYear.Add(visit.PlaceId, visit);
                            }
                        }

                        var queryResultLastRide = await GetRecordsAsync<DtoLastRideScore>(connection, _querySelectCurrentLastRideScores, new RowMappers.DtoLastRideScoreRowMapper(), parameters);

                        foreach (var visitDto in queryResultLastRide)
                        {
                            if (athleteTracks.ContainsKey(visitDto.lastExternalId))
                            {
                                var track = athleteTracks[visitDto.lastExternalId];

                                var visit = new Visit()
                                {
                                    PlaceId = visitDto.lastPlaceId,
                                    ExternalId = visitDto.lastExternalId,
                                    VisitedOn = visitDto.lastDate,
                                    Name = track.Name,
                                    Distance = track.Distance,
                                    Track = track.Coordinates
                                };

                                result.VisitedPlacesLastRide.Add(visit.PlaceId, visit);
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Unable to get Results due to: {ex.Message}");

                    throw;
                }
                finally
                {
                    await connection.CloseAsync();
                }


            }

            return result;
        }

        public async Task<Visit> GetMostRecentVisit(long athleteId)
        {
            using (var connection = new SqliteConnection(DbConnectionString))
            {
                await connection.OpenAsync();

                try
                {
                    var parameters = new Dictionary<string, object>()
                    {
                        { "@AthleteId", athleteId },
                        { "@MapName", _genericSettings.Id }
                    };

                    var queryResult = await GetRecordsAsync<DtoLastRideScore>(connection, _querySelectMostRecentVisit, new RowMappers.DtoLastRideScoreRowMapper(), parameters);

                    if (queryResult.Count > 0)
                    {
                        var result = new Visit()
                        {
                            PlaceId = queryResult[0].lastPlaceId,
                            ExternalId = queryResult[0].lastExternalId,
                            VisitedOn = queryResult[0].lastDate
                        };

                        return result;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Unable to get Visit due to: {ex.Message}");

                    throw;
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }

            return null;
        }

        public async Task<Dictionary<string, Track>> GetTracks(long athleteId)
        {
            var result = new Dictionary<string, Track>();

            using (var connection = new SqliteConnection(DbConnectionString))
            {
                await connection.OpenAsync();

                try
                {
                    var parameters = new Dictionary<string, object>()
                    {
                        { "@AthleteId", athleteId }
                    };

                    var queryResult = await GetRecordsAsync<DtoTrack>(connection, _querySelectTracks, new RowMappers.DtoTrackRowMapper(), parameters);

                    foreach (var trackDto in queryResult)
                    {
                        var track = new Track()
                        {
                            ExternalId = trackDto.tracExternalId,
                            Name = trackDto.tracName,
                            Distance = trackDto.tracDistance,
                            VisitedOn = trackDto.tracDate,
                            Coordinates = GeometryProducer.Instance.GetTrackFromWktLineString(trackDto.tracTrack),
                            Places = string.IsNullOrEmpty(trackDto.tracPlaces) ? new List<string>() : trackDto.tracPlaces.Split(',').ToList()
                        };

                        result.Add(track.ExternalId, track);
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Unable to get Tracks due to: {ex.Message}");

                    throw;
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }

            return result;
        }

        public override void Open()
        {
            var fileName = GetFileNameFromConnectionString();

            try
            {
                if (fileName != string.Empty)
                {
                    var folder = Path.GetDirectoryName(fileName);

                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    if (!File.Exists(fileName))
                    {
                        var processModule = Process.GetCurrentProcess().MainModule;
                        var masterDataFolder = Path.Combine(Path.GetDirectoryName(processModule?.FileName), "Master");

                        File.Copy(Path.Combine(masterDataFolder, "ltc2master.sqlite"), fileName);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Unable to copy master sql lite file: {ex.Message}");
            }
        }

        private string GetFileNameFromConnectionString()
        {
            var connectionStringParts = DbConnectionString.Split(';');

            foreach (var part in connectionStringParts)
            {
                if (part.ToLower().StartsWith("data source="))
                {
                    var sourceParts = part.Split('=');

                    return sourceParts[1];
                }
            }

            return string.Empty;
        }

        public async Task StoreScores(bool isRefresh, CalculationResult calculationResult)
        {
            using (var connection = new SqliteConnection(DbConnectionString))
            {
                await connection.OpenAsync();

                var transaction = (SqliteTransaction)await connection.BeginTransactionAsync();

                try
                {

                    var clearParameters = new Dictionary<string, object>()
                    {
                        { "@AthleteId", calculationResult.AthleteId },
                        { "@MapName", _genericSettings.Id }
                    };

                    if (isRefresh || calculationResult.VisitedPlacesAllTime.Count == 0)
                    {
                        await ExecuteNonQueryAsync(connection, _queryClearScores, clearParameters, transaction);
                    }

                    if (isRefresh || calculationResult.VisitedPlacesCurrentYear.Count == 0)
                    {
                        await ExecuteNonQueryAsync(connection, _queryClearCurrentYearScores, clearParameters, transaction);
                    }

                    if (calculationResult.UpdatedPlacesLastRide.Count > 0)
                    {
                        await ExecuteNonQueryAsync(connection, _queryClearLastRideScores, clearParameters, transaction);
                    }

                    foreach (var visit in calculationResult.UpdatedPlacesAllTime)
                    {
                        var upsertParameters = new Dictionary<string, object>()
                        {
                            { "@MapName", _genericSettings.Id },
                            { "@ExternalId", visit.ExternalId ?? string.Empty },
                            { "@AthleteId", calculationResult.AthleteId },
                            { "@Date", visit .VisitedOn.ToString("yyyy-MM-dd hh:mm.ss")},
                            { "@PlaceId", visit.PlaceId }
                        };

                        await ExecuteNonQueryAsync(connection, _queryUpsertScores, upsertParameters, transaction);
                    }

                    foreach (var visit in calculationResult.UpdatedPlacesCurrentYear)
                    {
                        var upsertParameters = new Dictionary<string, object>()
                        {
                            { "@MapName", _genericSettings.Id },
                            { "@ExternalId", visit.ExternalId ?? string.Empty },
                            { "@AthleteId", calculationResult.AthleteId },
                            { "@Date", visit .VisitedOn.ToString("yyyy-MM-dd hh:mm.ss")},
                            { "@PlaceId", visit.PlaceId }
                        };

                        await ExecuteNonQueryAsync(connection, _queryUpsertCurrentYearScores, upsertParameters, transaction);
                    }

                    foreach (var visit in calculationResult.UpdatedPlacesLastRide)
                    {
                        var insertParameters = new Dictionary<string, object>()
                        {
                            { "@MapName", _genericSettings.Id },
                            { "@ExternalId", visit.ExternalId ?? string.Empty },
                            { "@AthleteId", calculationResult.AthleteId },
                            { "@Date", visit .VisitedOn.ToString("yyyy-MM-dd hh:mm.ss")},
                            { "@PlaceId", visit.PlaceId }
                        };

                        await ExecuteNonQueryAsync(connection, _queryInsertLastRideScores, insertParameters, transaction);
                    }

                    foreach (var track in calculationResult.UpdatedTracks)
                    {
                        var upsertParameters = new Dictionary<string, object>()
                        {
                            { "@ExternalId", track.ExternalId },
                            { "@AthleteId", calculationResult.AthleteId },
                            { "@Name", track.Name },
                            { "@Track", GetTrackAsWkt(track.Coordinates) },
                            { "@Date", track.VisitedOn.ToString("yyyy-MM-dd hh:mm.ss") },
                            { "@Distance", track.Distance },
                            { "@Places", (track.Places == null || track.Places.Count == 0) ? string.Empty : string.Join(',', track.Places) },
                        };

                        await ExecuteNonQueryAsync(connection, _queryUpsertTracks, upsertParameters, transaction);
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Unable to store results due to: {ex.Message}");

                    await transaction.RollbackAsync();

                    throw;
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }
        }

        private string GetTrackAsWkt(List<List<double>> track)
        {
            if (track != null && track.Count > 1)
            {
                return GeometryProducer.Instance.CreateLinestringAsWktString(track);
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
