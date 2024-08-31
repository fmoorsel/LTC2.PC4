using LTC2.Shared.Database.Respositories;
using LTC2.Shared.Models.Domain;
using LTC2.Shared.Models.Dtos.SqlServer;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.StravaConnector.Interfaces;
using LTC2.Shared.StravaConnector.Stores.RowMappers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LTC2.Shared.StravaConnector.Stores
{
    public class DatabaseSesionStore : AbstractSqlRepository, ISessionStore
    {
        private readonly ILogger<DatabaseSesionStore> _logger;
        private readonly GenericSettings _genericSettings;

        public DatabaseSesionStore(ILogger<DatabaseSesionStore> logger, GenericSettings genericSettings)
        {
            _logger = logger;
            _genericSettings = genericSettings;

            DbConnectionString = _genericSettings.DatabaseConnectionString;
        }

        public async Task<Session> RetrieveAsync(long athleteId, Session currentSession = null)
        {
            var result = currentSession ?? new Session();

            try
            {
                if (athleteId > 0)
                {
                    var dbParameter = new DbParameter("@AthleteId", athleteId);
                    var dbParameters = new List<DbParameter>()
                    {
                        dbParameter
                    };

                    var queryResult = await GetRecordsByStoredProcedureAsync("spGetAthleteSessionInfo", new DtoSessionStoreRowMapper(), dbParameters);

                    if (queryResult != null && queryResult.Count > 0)
                    {
                        result = JsonConvert.DeserializeObject<Session>(queryResult[0].sessSessionInfo);
                    }
                }
                else
                {
                    _logger.LogWarning($"Missing athlete ID, not retrieving refresh token");
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unable to retrieve refresh token for {athleteId}: {e.Message}");
            }

            return result;
        }

        public Session Retrieve(long athleteId, Session currentSession = null)
        {
            var result = currentSession ?? new Session();

            try
            {
                if (athleteId > 0)
                {
                    var dbParameter = new DbParameter("@AthleteId", athleteId);
                    var dbParameters = new List<DbParameter>()
                    {
                        dbParameter
                    };

                    var queryResult = GetRecordsByStoredProcedure<DtoSessionStore>("spGetAthleteSessionInfo", new DtoSessionStoreRowMapper(), dbParameters);

                    if (queryResult != null && queryResult.Count > 0)
                    {
                        result = JsonConvert.DeserializeObject<Session>(queryResult[0].sessSessionInfo);
                    }
                }
                else
                {
                    _logger.LogWarning($"Missing athlete ID, not retrieving refresh token");
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unable to retrieve refresh token for {athleteId}: {e.Message}");
            }

            return result;
        }

        public void Store(Session session)
        {
            throw new NotImplementedException();
        }
    }
}
