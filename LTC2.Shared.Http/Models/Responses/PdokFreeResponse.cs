using Newtonsoft.Json;
using System.Collections.Generic;

namespace LTC2.Shared.Http.Models.Responses
{
    public class PdokFreeResponse
    {
        [JsonProperty("response")]
        public PdokResponseBody Response { get; set; }
    }

    public class PdokResponseBody
    {
        [JsonProperty("numFound")]
        public int NumFound { get; set; }

        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("docs")]
        public List<PdokDoc> Docs { get; set; } = new List<PdokDoc>();
    }

    public class PdokDoc
    {
        [JsonProperty("weergavenaam")]
        public string Weergavenaam { get; set; }

        /// <summary>WGS84 coordinate in WKT format: POINT(lon lat)</summary>
        [JsonProperty("centroide_ll")]
        public string CentroidLL { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }

        [JsonProperty("postcode")]
        public string Postcode { get; set; }

        [JsonProperty("woonplaatsnaam")]
        public string Woonplaatsnaam { get; set; }

        [JsonProperty("straatnaam")]
        public string Straatnaam { get; set; }

        [JsonProperty("huisnummer")]
        public int? Huisnummer { get; set; }
    }
}
