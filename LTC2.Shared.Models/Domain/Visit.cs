using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LTC2.Shared.Models.Domain
{
    public class Visit
    {
        [JsonIgnore]
        public Place Place { get; set; }

        public string PlaceId { get; set; }

        public DateTime VisitedOn { get; set; }

        public string Name { get; set; }

        public long Distance { get; set; }

        public string ExternalId { get; set; }

        public List<List<double>> Track { get; set; }

        public List<string> VisitedPlaces { get; set; } = new List<string>();

        public bool Updated { get; set; }
    }
}
