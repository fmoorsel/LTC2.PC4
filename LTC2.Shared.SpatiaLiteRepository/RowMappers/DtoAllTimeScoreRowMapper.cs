using LTC2.Shared.Database.Extensions;
using LTC2.Shared.Database.Interfaces;
using LTC2.Shared.Models.Dtos.SqlServer;
using System;
using System.Data;
using System.Globalization;

namespace LTC2.Shared.SpatiaLiteRepository.RowMappers
{
    public class DtoAllTimeScoreRowMapper : IRowMapper<DtoAllTimeScore>
    {
        public DtoAllTimeScore Map(IDataReader sqlreader)
        {
            var dto = new DtoAllTimeScore();

            var dateAsString = sqlreader.GetValue<string>("alltDate");
            var date = DateTime.ParseExact(dateAsString, "yyyy-MM-dd hh:mm.ss", CultureInfo.InvariantCulture);

            dto.alltId = sqlreader.GetValue<long>("alltId");
            dto.alltExternalId = sqlreader.GetValue<string>("alltExternalId");
            dto.alltAthleteId = sqlreader.GetValue<long>("alltAthleteId");
            dto.alltDate = date;
            dto.alltPlaceId = sqlreader.GetValue<string>("alltPlaceId");
            dto.alltMapName = sqlreader.GetValue<string>("alltMapName");

            return dto;
        }
    }
}
