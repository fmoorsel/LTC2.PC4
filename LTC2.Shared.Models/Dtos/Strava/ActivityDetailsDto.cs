using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LTC2.Shared.Models.Dtos.Strava
{
    public class ActivityDetailsDto
    {
        private static readonly string _latLngIndicator = "latlng";
        public static List<List<double>> GetCoordinates(List<ActivityDetailsDto> dtos)
        {
            var result = new List<List<double>>();

            var coordianteData = dtos.FirstOrDefault(d => d.Type == _latLngIndicator);

            if (coordianteData != null)
            {
                result.AddRange(coordianteData.Coordinates);
            }

            return result;
        }

        public string Type { get; set; }

        public List<object> Data { get; set; }


        public List<List<double>> Coordinates
        {
            get
            {
                if (Type == _latLngIndicator)
                {
                    var result = new List<List<double>>();

                    foreach (var d in Data)
                    {
                        var latStr = ((JArray)d)[0].ToString().Replace(",", ".");
                        var lngStr = ((JArray)d)[1].ToString().Replace(",", ".");

                        var lat = double.Parse(latStr, NumberFormatInfo.InvariantInfo);
                        var lon = double.Parse(lngStr, NumberFormatInfo.InvariantInfo);

                        var coordinate = new List<double>();

                        coordinate.Add(lon);
                        coordinate.Add(lat);

                        result.Add(coordinate);
                    }

                    return result;
                }
                else
                {
                    return new List<List<double>>();
                }
            }
        }
    }
}
