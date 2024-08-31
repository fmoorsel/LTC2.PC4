
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace LTC2.Shared.Models.Mapdefinitions
{
    public class BorderDescription
    {
        [JsonIgnoreAttribute]
        public Polygon Border { get; set; }

        public string BorderAsString { get; set; }

        [JsonIgnoreAttribute]
        public List<Polygon> Excludes { get; set; } = new List<Polygon>();

        public List<string> ExcludesAsString { get; set; } = new List<string>();

        public string Wkt { get; set; }

        [JsonIgnoreAttribute]
        public Polygon StandardizedizedPolygon { get; set; }
    }
}
