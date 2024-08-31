using LTC2.Shared.Database.Extensions;
using LTC2.Shared.Database.Interfaces;
using LTC2.Shared.Models.Dtos.SqlServer;
using System.Data;

namespace LTC2.Shared.SpatiaLiteRepository.RowMappers
{
    public class DtoMapRowMapper : IRowMapper<DtoMap>
    {
        public DtoMap Map(IDataReader sqlreader)
        {
            var dto = new DtoMap();

            dto.mapId = sqlreader.GetValue<long>("mapId");
            dto.mapName = sqlreader.GetValue<string>("mapName");
            dto.mapFeaturePointer = sqlreader.GetValue<string>("mapFeaturePointer");

            return dto;
        }
    }
}
