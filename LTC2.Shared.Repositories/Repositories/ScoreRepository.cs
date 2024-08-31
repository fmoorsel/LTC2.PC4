using LTC2.Shared.Database.Respositories;
using LTC2.Shared.Models.Domain;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.Repositories.Interfaces;
using LTC2.Shared.Repositories.RowMappers;
using LTC2.Shared.Utils.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace LTC2.Shared.Repositories.Repositories
{
    public class ScoreRepository : AbstractSqlRepository, IScoresRepository
    {
        private readonly ILogger<ScoreRepository> _logger;
        private readonly GenericSettings _genericSettings;

        public ScoreRepository(ILogger<ScoreRepository> logger, GenericSettings genericSettings)
        {
            _logger = logger;
            _genericSettings = genericSettings;

            DbConnectionString = _genericSettings.DatabaseConnectionString;
        }

        public void Open()
        {

        }

        public async Task<Visit> GetMostRecentVisit(long athleteId)
        {
            var dbParameterAthleteId = new DbParameter("@AthleteId", athleteId);
            var dbParameterMapName = new DbParameter("@MapName", _genericSettings.Id);

            var dbParameters = new List<DbParameter>()
            {
                dbParameterMapName,
                dbParameterAthleteId
            };

            var queryResult = await GetRecordsByStoredProcedureAsync("spGetMostRecentVisit", new DtoLastRideScoreRowMapper(), dbParameters);

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

            return null;
        }

        public async Task<CalculationResult> GetMostRecentResult(long athleteId)
        {
            var result = new CalculationResult()
            {
                AthleteId = athleteId
            };

            var athleteTracks = await GetTracks(athleteId);

            var dbParameterAthleteId = new DbParameter("@AthleteId", athleteId);
            var dbParameterMapName = new DbParameter("@MapName", _genericSettings.Id);

            var dbParameters = new List<DbParameter>()
            {
                dbParameterMapName,
                dbParameterAthleteId
            };

            var queryResultAlltime = await GetRecordsByStoredProcedureAsync("spGetVisits", new DtoAllTimeScoreRowMapper(), dbParameters);

            if (queryResultAlltime.Count > 0)
            {
                foreach (var visitDto in queryResultAlltime)
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

                var queryResultCurrentYear = await GetRecordsByStoredProcedureAsync("spGetCurrentYearVisits", new DtoCurrentYearScoreRowMapper(), dbParameters);

                foreach (var visitDto in queryResultCurrentYear)
                {
                    if (visitDto.currDate.Year == DateTime.UtcNow.Year)
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

                var queryResultLastRide = await GetRecordsByStoredProcedureAsync("spGetLastRideVisits", new DtoLastRideScoreRowMapper(), dbParameters);

                foreach (var visitDto in queryResultLastRide)
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

            return result;
        }

        public async Task<Dictionary<string, Track>> GetTracks(long athleteId)
        {
            var result = new Dictionary<string, Track>();

            var dbParameterAthleteId = new DbParameter("@AthleteId", athleteId);

            var dbParameters = new List<DbParameter>()
            {
                dbParameterAthleteId
            };

            var queryResult = await GetRecordsByStoredProcedureAsync("spGetTracks", new DtoTrackRowMapper(), dbParameters);

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

            return result;
        }

        public async Task<List<Track>> GetAlltimeTracksForAllPlaces(long athleteId)
        {
            var result = new List<Track>();

            var dbParameterAthleteId = new DbParameter("@AthleteId", athleteId);
            var dbParameterMapName = new DbParameter("@MapName", _genericSettings.Id);

            var dbParameters = new List<DbParameter>()
            {
                dbParameterMapName,
                dbParameterAthleteId
            };

            var queryResult = await GetRecordsByStoredProcedureAsync("spGetAlltimeTracksForAllPlaces", new DtoTrackRowMapper(), dbParameters);

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

            return result;
        }

        public async Task<Track> GetAlltimeTrackForPlace(long athleteId, string placeId, bool detailed)
        {
            var dbParameterAthleteId = new DbParameter("@AthleteId", athleteId);
            var dbParameterMapName = new DbParameter("@MapName", _genericSettings.Id);
            var dbParameterPlace = new DbParameter("@Place", placeId);

            var dbParameters = new List<DbParameter>()
            {
                dbParameterMapName,
                dbParameterAthleteId,
                dbParameterPlace
            };

            var queryResult = await GetRecordsByStoredProcedureAsync("spGetAlltimeTrackForPlace", new DtoTrackForAlltimePlaceRowMappers(), dbParameters);

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

                return track;
            }

            return null;
        }

        public async Task StoreScores(bool isRefresh, CalculationResult calculationResult)
        {
            var dataTableAlltime = CreateVisitsDataTable();

            foreach (var visit in calculationResult.UpdatedPlacesAllTime)
            {
                AddToVisitDataTable(dataTableAlltime, calculationResult.AthleteId, visit);
            }

            var dbParameterAllTime = new DbParameter("@Scores", dataTableAlltime);

            var dbParametersAlltime = new List<DbParameter>()
            {
                dbParameterAllTime
            };

            var dataTableCurrentYear = CreateVisitsDataTable();

            foreach (var visit in calculationResult.UpdatedPlacesCurrentYear)
            {
                AddToVisitDataTable(dataTableCurrentYear, calculationResult.AthleteId, visit);
            }

            var dbParameterCurrentYear = new DbParameter("@Scores", dataTableCurrentYear);

            var dbParametersCurrentYear = new List<DbParameter>()
            {
                dbParameterCurrentYear
            };

            var dataTableLastRide = CreateVisitsDataTable();

            foreach (var visit in calculationResult.UpdatedPlacesLastRide)
            {
                AddToVisitDataTable(dataTableLastRide, calculationResult.AthleteId, visit);
            }

            var dbParameterLastRide = new DbParameter("@Scores", dataTableLastRide);

            var dbParametersLastRide = new List<DbParameter>()
            {
                dbParameterLastRide
            };

            var dbParameterAthleteId = new DbParameter("@AthleteId", calculationResult.AthleteId);
            var dbParameterMapName = new DbParameter("@MapName", _genericSettings.Id);


            var dbParametersAthleteId = new List<DbParameter>()
            {
                dbParameterAthleteId
            };

            var dbParametersAthleteIdAndMapName = new List<DbParameter>()
            {
                dbParameterAthleteId,
                dbParameterMapName
            };

            using (var sqlConnection = new SqlConnection(DbConnectionString))
            {
                sqlConnection.Open();

                var transaction = await sqlConnection.BeginTransactionAsync() as SqlTransaction;

                try
                {
                    if (isRefresh || calculationResult.VisitedPlacesAllTime.Count == 0)
                    {
                        await ExecuteStoredProcedureNonQueryInTransactionAsync(sqlConnection, transaction, "spClearScores", dbParametersAthleteIdAndMapName);
                    }

                    if (calculationResult.UpdatedPlacesAllTime.Count > 0)
                    {
                        await ExecuteStoredProcedureNonQueryInTransactionAsync(sqlConnection, transaction, "spUpsertScores", dbParametersAlltime);
                    }

                    if (isRefresh || calculationResult.VisitedPlacesCurrentYear.Count == 0)
                    {
                        await ExecuteStoredProcedureNonQueryInTransactionAsync(sqlConnection, transaction, "spClearCurrentYearScores", dbParametersAthleteIdAndMapName);
                    }

                    if (calculationResult.UpdatedPlacesCurrentYear.Count > 0)
                    {
                        await ExecuteStoredProcedureNonQueryInTransactionAsync(sqlConnection, transaction, "spUpsertCurrentYearScores", dbParametersCurrentYear);
                    }


                    if (calculationResult.UpdatedPlacesLastRide.Count > 0)
                    {
                        await ExecuteStoredProcedureNonQueryInTransactionAsync(sqlConnection, transaction, "spClearLastRideScores", dbParametersAthleteIdAndMapName);

                        await ExecuteStoredProcedureNonQueryInTransactionAsync(sqlConnection, transaction, "spInsertLastRideScores", dbParametersLastRide);
                    }

                    var tracks = calculationResult.UpdatedTracks;

                    if (tracks.Count > 0)
                    {
                        await UpsertTracks(tracks, calculationResult.AthleteId, sqlConnection, transaction);
                    }

                    await ExecuteStoredProcedureNonQueryInTransactionAsync(sqlConnection, transaction, "spSanitizeTracks", dbParametersAthleteId);

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Unable to store results due to: {ex.Message}");

                    await transaction.RollbackAsync();

                    throw;
                }
            }
        }

        private readonly string ColumnExternalId = "ExternalId";
        private readonly string ColumnMapName = "MapName";
        private readonly string ColumnName = "Name";
        private readonly string ColumnAthleteId = "AthleteId";
        private readonly string ColumnDate = "Date";
        private readonly string ColumnTrack = "Track";
        private readonly string ColumnPlaceId = "PlaceId";
        private readonly string ColumnDistance = "Distance";
        private readonly string ColumnPlaces = "Places";

        private void AddToVisitDataTable(DataTable dataTable, long athleteId, Visit visit)
        {
            var row = dataTable.NewRow();

            row[ColumnExternalId] = visit.ExternalId ?? string.Empty;
            row[ColumnMapName] = _genericSettings.Id;
            row[ColumnAthleteId] = athleteId;
            row[ColumnDate] = visit.VisitedOn;
            row[ColumnPlaceId] = visit.PlaceId;

            dataTable.Rows.Add(row);
        }

        private void AddToTrackDataTable(DataTable dataTable, long athleteId, Track track)
        {
            var row = dataTable.NewRow();

            row[ColumnExternalId] = track.ExternalId;
            row[ColumnName] = track.Name ?? string.Empty;
            row[ColumnAthleteId] = athleteId;
            row[ColumnDate] = track.VisitedOn;
            row[ColumnTrack] = GetTrackAsWkt(track.Coordinates);
            row[ColumnDistance] = track.Distance;
            row[ColumnPlaces] = (track.Places == null || track.Places.Count == 0) ? string.Empty : string.Join(',', track.Places);

            dataTable.Rows.Add(row);
        }

        private DataTable CreateVisitsDataTable()
        {
            var dataTable = new DataTable();

            dataTable.Columns.Add(ColumnExternalId, typeof(string));
            dataTable.Columns.Add(ColumnMapName, typeof(string));
            dataTable.Columns.Add(ColumnAthleteId, typeof(Int64));
            dataTable.Columns.Add(ColumnDate, typeof(DateTime));
            dataTable.Columns.Add(ColumnPlaceId, typeof(string));

            return dataTable;
        }

        private DataTable CreateTracksDataTable()
        {
            var dataTable = new DataTable();

            dataTable.Columns.Add(ColumnExternalId, typeof(string));
            dataTable.Columns.Add(ColumnName, typeof(string));
            dataTable.Columns.Add(ColumnAthleteId, typeof(Int64));
            dataTable.Columns.Add(ColumnDate, typeof(DateTime));
            dataTable.Columns.Add(ColumnTrack, typeof(string));
            dataTable.Columns.Add(ColumnDistance, typeof(Int64));
            dataTable.Columns.Add(ColumnPlaces, typeof(string));

            return dataTable;
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

        private async Task UpsertTracks(List<Track> tracks, long athleteId, SqlConnection connection, SqlTransaction transaction)
        {
            var dataTableTracks = CreateTracksDataTable();

            foreach (var track in tracks)
            {
                AddToTrackDataTable(dataTableTracks, athleteId, track);
            }

            var dbParameter = new DbParameter("@Scores", dataTableTracks);

            var dbParameters = new List<DbParameter>()
            {
                dbParameter
            };

            await ExecuteStoredProcedureNonQueryInTransactionAsync(connection, transaction, "spUpsertTracks", dbParameters);
        }
    }
}
