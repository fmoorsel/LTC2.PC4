using LTC2.Shared.Database.Extensions;
using LTC2.Shared.Database.Interfaces;
using LTC2.Shared.Models.Dtos.SqlServer;
using System;
using System.Data;
using System.Globalization;

namespace LTC2.Shared.SpatiaLiteRepository.RowMappers
{
    public class DtoLastRideScoreRowMapper : IRowMapper<DtoLastRideScore>
    {
        public DtoLastRideScore Map(IDataReader sqlreader)
        {
            var dto = new DtoLastRideScore();

            var dateAsString = sqlreader.GetValue<string>("lastDate");
            var date = DateTime.ParseExact(dateAsString, "yyyy-MM-dd hh:mm.ss", CultureInfo.InvariantCulture);

            dto.lastId = sqlreader.GetValue<long>("lastId");
            dto.lastExternalId = sqlreader.GetValue<string>("lastExternalId");
            dto.lastAthleteId = sqlreader.GetValue<long>("lastAthleteId");
            dto.lastDate = date;
            dto.lastPlaceId = sqlreader.GetValue<string>("lastPlaceId");
            dto.lastMapName = sqlreader.GetValue<string>("lastMapName");

            return dto;
        }
    }
}
