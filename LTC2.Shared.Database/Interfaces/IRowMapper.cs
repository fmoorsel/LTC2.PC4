using System.Data;

namespace LTC2.Shared.Database.Interfaces
{
    public interface IRowMapper<T> where T : class
    {
        T Map(IDataReader sqlreader);
    }
}
