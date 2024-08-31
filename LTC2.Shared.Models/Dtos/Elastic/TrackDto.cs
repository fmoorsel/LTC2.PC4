using System.Collections.Generic;

namespace LTC2.Shared.Models.Dtos.Elastic
{
    public class TrackDto
    {
        public string Type { get; set; } = "LineString";

        public List<List<double>> Coordinates { get; set; } = new List<List<double>>();
    }
}
