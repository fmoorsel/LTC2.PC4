using LTC2.Shared.Database.Extensions;
using LTC2.Shared.Database.Interfaces;
using System.Collections.Generic;
using System.Data;

namespace LTC2.Shared.SpatiaLiteRepository.RowMappers
{
    public class PointRowMapper : IRowMapper<List<double>>
    {
        public List<double> Map(IDataReader sqlreader)
        {
            var x = sqlreader.GetValue<double>("x");
            var y = sqlreader.GetValue<double>("y");

            return new List<double> { x, y };
        }
    }
}
