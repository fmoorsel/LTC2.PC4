using System;

namespace LTC2.Shared.Models.Dtos.SqlServer
{
    public class DtoCurrentYearScore
    {
        public long currId { get; set; }

        public string currMapName { get; set; }

        public string currExternalId { get; set; }

        public long currAthleteId { get; set; }

        public DateTime currDate { get; set; }

        public string currPlaceId { get; set; }

    }
}
