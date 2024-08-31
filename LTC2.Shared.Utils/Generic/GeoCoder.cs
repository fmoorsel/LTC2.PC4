using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LTC2.Shared.Utils.Generic
{
    public class GeoCoder
    {
        public static LineString DecodeToLineString(GeometryFactory factory, string encodedPoints)
        {
            var points = Decode(encodedPoints);

            if (points.Count() > 0)
            {
                return factory.CreateLineString(points.ToArray());
            }
            else
            {
                return null;
            }
        }

        public static IEnumerable<Coordinate> Decode(string encodedPoints)
        {
            char[] polylineChars = encodedPoints.ToCharArray();
            int index = 0;

            int currentLat = 0;
            int currentLng = 0;
            int next5bits;
            int sum;
            int shifter;

            while (index < polylineChars.Length)
            {
                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = (int)polylineChars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length)
                    break;

                currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                sum = 0;
                shifter = 0;
                do
                {
                    next5bits = (int)polylineChars[index++] - 63;
                    sum |= (next5bits & 31) << shifter;
                    shifter += 5;
                } while (next5bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length && next5bits >= 32)
                    break;

                currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                yield return new Coordinate()
                {
                    Y = Convert.ToDouble(currentLat) / 1E5,
                    X = Convert.ToDouble(currentLng) / 1E5,
                };
            }
        }

        public static List<List<double>> DecodeToTrack(string encodedPoints)
        {
            var coordinates = new List<List<double>>();

            if (!string.IsNullOrEmpty(encodedPoints))
            {
                var decoded = Decode(encodedPoints);

                coordinates.AddRange(decoded.Select(c => new List<double> { c.X, c.Y }));
            }

            return coordinates;
        }
    }
}
