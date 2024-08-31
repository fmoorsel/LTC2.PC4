using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace LTC2.Shared.Models.Mapdefinitions
{
    public class PlaceDefination
    {
        public string ID { get; set; }

        public string Name { get; set; }

        public string ExternalId { get; set; }

        public List<BorderDescription> Borders { get; set; } = new List<BorderDescription>();

        [JsonIgnoreAttribute]
        public Polygon HitArea { get; set; }

        public string HitAreaAsWkt { get; set; }

        public bool Deprecated { get; set; }

        public string Group { get; set; }


        [JsonIgnoreAttribute]

        public List<List<Polygon>> BorderPolygons
        {
            get
            {
                var result = new List<List<Polygon>>();

                foreach (var border in Borders)
                {
                    var borderDef = new List<Polygon>();

                    borderDef.Add(border.Border);

                    foreach (var hole in border.Excludes)
                    {
                        borderDef.Add(hole);
                    }

                    result.Add(borderDef);
                }

                return result;
            }
        }
    }
}
