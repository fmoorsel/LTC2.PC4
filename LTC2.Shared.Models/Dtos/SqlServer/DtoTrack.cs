using System;

namespace LTC2.Shared.Models.Dtos.SqlServer
{
    public class DtoTrack
    {
        public long tracId { get; set; }

        public string tracExternalId { get; set; }

        public string tracName { get; set; }

        public long tracAthleteId { get; set; }

        public DateTime tracDate { get; set; }

        public string tracTrack { get; set; }

        public long tracDistance { get; set; }

        public string tracPlaces { get; set; }

    }
}
