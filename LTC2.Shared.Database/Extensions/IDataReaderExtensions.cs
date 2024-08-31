using System;
using System.Data;

namespace LTC2.Shared.Database.Extensions
{
    public static class IDataReaderExtensions
    {
        public static T GetValue<T>(this IDataReader rdr, string columnName)
        {
            try
            {
                var index = rdr.GetOrdinal(columnName);
                var data = rdr.GetValue(index);

                if (data != null && !string.IsNullOrEmpty(data.ToString()))
                {
                    return (T)data;
                }

                return default(T);
            }
            catch (IndexOutOfRangeException)
            {
                return default(T);
            }
        }
    }

}
