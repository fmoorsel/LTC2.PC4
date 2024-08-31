using Newtonsoft.Json;
using System.Collections.Generic;

namespace LTC2.Shared.StravaConnector.Models
{
    public class StravaDistanceStream
    {
        [JsonProperty("series_type")]
        public string SeriesType { get; set; }

        public List<double> Data { get; set; }
    }
}
