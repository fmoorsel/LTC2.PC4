using System.Collections.Generic;

namespace LTC2.Shared.Models.Domain
{
    public class Activity
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string AthleteId { get; set; }

        public List<List<double>> Track { get; set; }

    }
}
