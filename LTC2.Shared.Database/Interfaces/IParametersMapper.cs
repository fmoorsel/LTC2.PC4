using Microsoft.Data.SqlClient;
using System.Data;

namespace LTC2.Shared.Database.Interfaces
{
    public interface IParameterMapper
    {
        void Map(SqlParameterCollection parameters, DataTable dataTable);
    }

}
