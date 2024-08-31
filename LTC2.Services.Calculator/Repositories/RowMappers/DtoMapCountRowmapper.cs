using LTC2.Shared.Database.Extensions;
using LTC2.Shared.Database.Interfaces;
using LTC2.Shared.Models.Dtos.SqlServer;
using System.Data;

namespace LTC2.Services.Calculator.Repositories.RowMappers
{
    public class DtoMapCountRowmapper : IRowMapper<DtoMapCount>
    {
        public DtoMapCount Map(IDataReader sqlreader)
        {
            var dtoMapCount = new DtoMapCount();

            dtoMapCount.mapCount = sqlreader.GetValue<int>("mapCount");

            return dtoMapCount;
        }
    }
}
