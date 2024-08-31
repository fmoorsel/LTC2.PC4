using System;

namespace LTC2.Shared.Models.Dtos.SqlServer
{
    public class DtoAllTimeScore
    {
        public long alltId { get; set; }

        public string alltMapName { get; set; }

        public string alltExternalId { get; set; }

        public long alltAthleteId { get; set; }

        public DateTime alltDate { get; set; }

        public string alltPlaceId { get; set; }

    }
}
