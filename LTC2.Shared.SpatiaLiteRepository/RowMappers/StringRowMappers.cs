using LTC2.Shared.Database.Extensions;
using LTC2.Shared.Database.Interfaces;
using System.Data;

namespace LTC2.Shared.SpatiaLiteRepository.RowMappers
{
    public class StringRowMappers : IRowMapper<string>
    {
        public string Map(IDataReader sqlreader)
        {
            return sqlreader.GetValue<string>("name");
        }


    }
}
