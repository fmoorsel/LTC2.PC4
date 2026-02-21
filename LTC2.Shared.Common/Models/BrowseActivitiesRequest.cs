using System;

namespace LTC2.Shared.Common.Models
{
    public class BrowseActivitiesRequest
    {
        public long AthleteId { get; set; }

        public bool BypassCache { get; set; }

        public DateTime? After { get; set; } = new DateTime(2000, 1, 1);
    }
}
