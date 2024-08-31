using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LTC2.Shared.Models.Domain
{
    public class Track
    {
        public Track()
        {
        }

        public Track(Visit visit)
        {
            ExternalId = visit.ExternalId;
            Name = visit.Name;
            Coordinates = visit.Track;
            Distance = visit.Distance;
            VisitedOn = visit.VisitedOn;
            Updated = visit.Updated;
            Places = visit.VisitedPlaces;
        }

        public DateTime VisitedOn { get; set; }

        public string Name { get; set; }

        public long Distance { get; set; }

        public string ExternalId { get; set; }

        public List<List<double>> Coordinates { get; set; }

        public List<string> Places { get; set; }

        [JsonIgnore]
        public bool Updated { get; set; }
    }
}
