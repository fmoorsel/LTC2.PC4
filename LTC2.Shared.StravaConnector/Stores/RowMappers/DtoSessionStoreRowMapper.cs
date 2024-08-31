using LTC2.Shared.Database.Extensions;
using LTC2.Shared.Database.Interfaces;
using LTC2.Shared.Models.Dtos.SqlServer;
using System.Data;

namespace LTC2.Shared.StravaConnector.Stores.RowMappers
{
    public class DtoSessionStoreRowMapper : IRowMapper<DtoSessionStore>
    {
        public DtoSessionStore Map(IDataReader sqlreader)
        {
            var dto = new DtoSessionStore();

            dto.sessId = sqlreader.GetValue<long>("sessId");
            dto.sessAthleteId = sqlreader.GetValue<long>("sessAthleteId");
            dto.sessSessionInfo = sqlreader.GetValue<string>("sessSessionInfo");

            return dto;

        }
    }
}
