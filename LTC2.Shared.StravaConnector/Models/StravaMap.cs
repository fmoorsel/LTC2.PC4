using Newtonsoft.Json;

namespace LTC2.Shared.StravaConnector.Models
{
    public class StravaMap
    {
        public string Id { get; set; }

        public string Polyline { get; set; }

        [JsonProperty("summary_polyline")]
        public string SummaryPolyline { get; set; }

    }
}
