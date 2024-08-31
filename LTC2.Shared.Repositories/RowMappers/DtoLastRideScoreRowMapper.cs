using LTC2.Shared.Database.Extensions;
using LTC2.Shared.Database.Interfaces;
using LTC2.Shared.Models.Dtos.SqlServer;
using System;
using System.Data;

namespace LTC2.Shared.Repositories.RowMappers
{
    public class DtoLastRideScoreRowMapper : IRowMapper<DtoLastRideScore>
    {
        public DtoLastRideScore Map(IDataReader sqlreader)
        {
            var dto = new DtoLastRideScore();

            dto.lastId = sqlreader.GetValue<long>("lastId");
            dto.lastExternalId = sqlreader.GetValue<string>("lastExternalId");
            dto.lastAthleteId = sqlreader.GetValue<long>("lastAthleteId");
            dto.lastDate = sqlreader.GetValue<DateTime>("lastDate");
            dto.lastPlaceId = sqlreader.GetValue<string>("lastPlaceId");
            dto.lastMapName = sqlreader.GetValue<string>("lastMapName");

            return dto;
        }
    }
}
