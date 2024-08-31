using LTC2.Shared.Database.Extensions;
using LTC2.Shared.Database.Interfaces;
using LTC2.Shared.Models.Dtos.SqlServer;
using System.Data;

namespace LTC2.Services.Calculator.Repositories.RowMappers
{
    public class DtoActivityIdRowMapper : IRowMapper<DtoActivityId>
    {
        public DtoActivityId Map(IDataReader sqlreader)
        {
            var dtoActivityId = new DtoActivityId();

            dtoActivityId.actiId = sqlreader.GetValue<long>("actiId");

            return dtoActivityId;
        }
    }
}
