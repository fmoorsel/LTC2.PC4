using LTC2.Shared.Database.Extensions;
using LTC2.Shared.Database.Interfaces;
using LTC2.Shared.Models.Dtos.SqlServer;
using System;
using System.Data;
using System.Globalization;

namespace LTC2.Shared.SpatiaLiteRepository.RowMappers
{
    public class DtoTrackRowMapper : IRowMapper<DtoTrack>
    {
        public DtoTrack Map(IDataReader sqlreader)
        {
            var dto = new DtoTrack();

            var dateAsString = sqlreader.GetValue<string>("tracDate");
            var date = DateTime.ParseExact(dateAsString, "yyyy-MM-dd hh:mm.ss", CultureInfo.InvariantCulture);

            dto.tracId = sqlreader.GetValue<long>("tracId");
            dto.tracExternalId = sqlreader.GetValue<string>("tracExternalId");
            dto.tracAthleteId = sqlreader.GetValue<long>("tracAthleteId");
            dto.tracDate = date;
            dto.tracName = sqlreader.GetValue<string>("tracName");
            dto.tracTrack = sqlreader.GetValue<string>("tracTrack");
            dto.tracDistance = sqlreader.GetValue<long>("tracDistance");
            dto.tracPlaces = sqlreader.GetValue<string>("tracPlaces");

            return dto;
        }
    }
}
