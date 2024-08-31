using LTC2.Shared.Database.Extensions;
using LTC2.Shared.Database.Interfaces;
using LTC2.Shared.Models.Dtos.SqlServer;
using System;
using System.Data;

namespace LTC2.Shared.Repositories.RowMappers
{
    public class DtoTrackRowMapper : IRowMapper<DtoTrack>
    {
        public DtoTrack Map(IDataReader sqlreader)
        {
            var dto = new DtoTrack();

            dto.tracId = sqlreader.GetValue<long>("tracId");
            dto.tracExternalId = sqlreader.GetValue<string>("tracExternalId");
            dto.tracAthleteId = sqlreader.GetValue<long>("tracAthleteId");
            dto.tracDate = sqlreader.GetValue<DateTime>("tracDate");
            dto.tracName = sqlreader.GetValue<string>("tracName");
            dto.tracTrack = sqlreader.GetValue<string>("tracTrack");
            dto.tracDistance = sqlreader.GetValue<long>("tracDistance");
            dto.tracPlaces = sqlreader.GetValue<string>("tracPlaces");

            return dto;
        }
    }
}
