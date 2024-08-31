using LTC2.Shared.Database.Extensions;
using LTC2.Shared.Database.Interfaces;
using LTC2.Shared.Models.Dtos.SqlServer;
using System.Data;

namespace LTC2.Shared.Repositories.RowMappers
{
    public class DtoTrackForAlltimePlaceRowMappers : IRowMapper<DtoTrackForAlltimePlace>
    {
        public DtoTrackForAlltimePlace Map(IDataReader sqlreader)
        {
            var dtoTrackRowMapper = new DtoTrackRowMapper();
            var dtoTrack = dtoTrackRowMapper.Map(sqlreader);

            var dtoTrackForAlltimePlace = new DtoTrackForAlltimePlace(dtoTrack)
            {
                alltId = sqlreader.GetValue<long>("alltId"),
                alltPlaceId = sqlreader.GetValue<string>("alltPlaceId")
            };

            return dtoTrackForAlltimePlace;
        }
    }
}
