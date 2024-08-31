using LTC2.Shared.Database.Extensions;
using LTC2.Shared.Database.Interfaces;
using LTC2.Shared.Models.Dtos.SqlServer;
using System;
using System.Data;

namespace LTC2.Shared.Repositories.RowMappers
{
    public class DtoAllTimeScoreRowMapper : IRowMapper<DtoAllTimeScore>
    {
        public DtoAllTimeScore Map(IDataReader sqlreader)
        {
            var dto = new DtoAllTimeScore();

            dto.alltId = sqlreader.GetValue<long>("alltId");
            dto.alltExternalId = sqlreader.GetValue<string>("alltExternalId");
            dto.alltAthleteId = sqlreader.GetValue<long>("alltAthleteId");
            dto.alltDate = sqlreader.GetValue<DateTime>("alltDate");
            dto.alltPlaceId = sqlreader.GetValue<string>("alltPlaceId");
            dto.alltMapName = sqlreader.GetValue<string>("alltMapName");

            return dto;
        }
    }
}
