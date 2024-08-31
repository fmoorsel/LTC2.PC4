using LTC2.Shared.Database.Extensions;
using LTC2.Shared.Database.Interfaces;
using LTC2.Shared.Models.Dtos.SqlServer;
using System.Data;

namespace LTC2.Shared.Repositories.RowMappers
{
    public class DtoProfileRowMapper : IRowMapper<DtoProfile>
    {
        public DtoProfile Map(IDataReader sqlreader)
        {
            var dto = new DtoProfile();

            dto.profId = sqlreader.GetValue<long>("profId");
            dto.profAthleteId = sqlreader.GetValue<long>("profAthleteId");
            dto.profEmail = sqlreader.GetValue<string>("profEmail");

            return dto;
        }
    }
}
