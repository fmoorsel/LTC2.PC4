using LTC2.Shared.Database.Respositories;
using LTC2.Shared.Models.Domain;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.Repositories.Interfaces;
using LTC2.Shared.Repositories.RowMappers;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LTC2.Shared.Repositories.Repositories
{
    public class InternalProfileRepository : AbstractSqlRepository, IInternalProfileRepository
    {
        private readonly ILogger<InternalProfileRepository> _logger;
        private readonly GenericSettings _genericSettings;

        public InternalProfileRepository(ILogger<InternalProfileRepository> logger, GenericSettings genericSettings)
        {
            _logger = logger;
            _genericSettings = genericSettings;

            DbConnectionString = _genericSettings.DatabaseConnectionString;
        }

        public async Task<InternalProfile> GetInternalProfile(long athleteId)
        {
            var result = new InternalProfile();

            if (athleteId > 0)
            {
                var dbParameterAthleteId = new DbParameter("@AthleteId", athleteId);

                var dbParameters = new List<DbParameter>()
                {
                    dbParameterAthleteId
                };

                var queryResult = await GetRecordsByStoredProcedureAsync("spGetAthleteProfile", new DtoProfileRowMapper(), dbParameters);

                if (queryResult.Count > 0)
                {
                    result.AthleteId = queryResult[0].profAthleteId;
                    result.Email = queryResult[0].profEmail;
                }
            }

            return result;
        }

        public async Task UpsertInternalProfile(InternalProfile profile)
        {
            if (profile.AthleteId > 0 && profile.Email != null)
            {
                var dbParameterAthleteId = new DbParameter("@AthleteId", profile.AthleteId);
                var dbParameterEmail = new DbParameter("@Email", profile.Email);


                var dbParameters = new List<DbParameter>()
                {
                    dbParameterAthleteId,
                    dbParameterEmail
                };

                await ExecuteStoredProcedureNonQueryAsync("spUpsertAthleteProfile", dbParameters);
            }
        }
    }
}
