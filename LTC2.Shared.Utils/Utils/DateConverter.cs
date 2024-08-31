using System;

namespace LTC2.Shared.Utils.Utils
{
    public class DateConverter
    {
        public static long GetSecondsSinceUnixEpoch(DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date.ToUniversalTime() - epoch).TotalSeconds);
        }
    }
}
