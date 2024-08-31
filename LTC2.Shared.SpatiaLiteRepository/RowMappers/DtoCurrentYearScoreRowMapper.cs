using LTC2.Shared.Database.Extensions;
using LTC2.Shared.Database.Interfaces;
using LTC2.Shared.Models.Dtos.SqlServer;
using System;
using System.Data;
using System.Globalization;

namespace LTC2.Shared.SpatiaLiteRepository.RowMappers
{
    public class DtoCurrentYearScoreRowMapper : IRowMapper<DtoCurrentYearScore>
    {
        public DtoCurrentYearScore Map(IDataReader sqlreader)
        {
            var dto = new DtoCurrentYearScore();

            var dateAsString = sqlreader.GetValue<string>("currDate");
            var date = DateTime.ParseExact(dateAsString, "yyyy-MM-dd hh:mm.ss", CultureInfo.InvariantCulture);

            dto.currId = sqlreader.GetValue<long>("currId");
            dto.currExternalId = sqlreader.GetValue<string>("currExternalId");
            dto.currAthleteId = sqlreader.GetValue<long>("currAthleteId");
            dto.currDate = date;
            dto.currPlaceId = sqlreader.GetValue<string>("currPlaceId");
            dto.currMapName = sqlreader.GetValue<string>("currMapName");

            return dto;
        }
    }
}
