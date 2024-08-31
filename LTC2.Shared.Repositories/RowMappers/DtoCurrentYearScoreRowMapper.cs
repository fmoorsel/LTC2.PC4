using LTC2.Shared.Database.Extensions;
using LTC2.Shared.Database.Interfaces;
using LTC2.Shared.Models.Dtos.SqlServer;
using System;
using System.Data;

namespace LTC2.Shared.Repositories.RowMappers
{
    public class DtoCurrentYearScoreRowMapper : IRowMapper<DtoCurrentYearScore>
    {
        public DtoCurrentYearScore Map(IDataReader sqlreader)
        {
            var dto = new DtoCurrentYearScore();

            dto.currId = sqlreader.GetValue<long>("currId");
            dto.currExternalId = sqlreader.GetValue<string>("currExternalId");
            dto.currAthleteId = sqlreader.GetValue<long>("currAthleteId");
            dto.currDate = sqlreader.GetValue<DateTime>("currDate");
            dto.currPlaceId = sqlreader.GetValue<string>("currPlaceId");
            dto.currMapName = sqlreader.GetValue<string>("currMapName");

            return dto;
        }
    }
}
