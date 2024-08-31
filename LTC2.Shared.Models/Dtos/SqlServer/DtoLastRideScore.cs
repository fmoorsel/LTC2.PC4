using System;

namespace LTC2.Shared.Models.Dtos.SqlServer
{
    public class DtoLastRideScore
    {
        public long lastId { get; set; }

        public string lastMapName { get; set; }

        public string lastExternalId { get; set; }

        public long lastAthleteId { get; set; }

        public DateTime lastDate { get; set; }

        public string lastPlaceId { get; set; }

    }
}
