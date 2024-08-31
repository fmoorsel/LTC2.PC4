using LTC2.Shared.Utils.Utils;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace LTC2.Shared.ActivityFormats.Json.Utils
{
    public static class JsonCoordinatesUtils
    {
        public static List<LineString> CreateLinestringForForTrack(string fileName)
        {
            var result = new List<LineString>();

            try
            {
                var content = File.ReadAllText(fileName);

                var values = content.IndexOf("\\\"values\\\"");
                var fields = content.IndexOf("\\\"fields\\\"", values);

                var relevantContent = content.Substring(values + 11, fields - values - 3 - 11);

                var a = JsonConvert.DeserializeObject<List<object>>(relevantContent);
                var coordinates = new List<List<double>>();

                foreach (var item in a)
                {
                    var x = item.ToString();

                    var start = x.IndexOf('[', 1);
                    var end = x.IndexOf(']');

                    var coordinatesJson = x.Substring(start, end - start + 1);
                    var coordinate = JsonConvert.DeserializeObject<List<double>>(coordinatesJson);

                    coordinates.Add(coordinate);
                }

                if (coordinates.Count > 1)
                {
                    var lineString = GeometryProducer.Instance.CreateLinestring(coordinates);
                    result.Add(lineString);
                }

            }
            catch (Exception)
            {
                //ignore
            }

            return result;
        }
    }
}
