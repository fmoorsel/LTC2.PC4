using System;

namespace LTC2.Shared.StravaConnector.Models.Requests
{
    public class GetActivitiesRequest
    {
        public long AthleteId { get; set; }

        public int Page { get; set; }

        public int PerPage { get; set; }

        public bool BypassCache { get; set; }

        public DateTime? Before { get; set; }

        public DateTime? After { get; set; } = new DateTime(2000, 1, 1);

    }
}
