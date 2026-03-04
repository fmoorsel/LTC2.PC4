using System;

namespace LTC2.Shared.RideWithGpsConnector.Models.Requests
{
    public class GetActivitiesRequest
    {
        public DateTime After { get; set; } = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public bool BypassCache { get; set; }
    }
}
