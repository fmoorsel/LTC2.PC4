using Gpx;
using LTC2.Shared.ActivityFormats.Gpx.Models.Generated;
using LTC2.Shared.Utils.Utils;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace LTC2.Shared.ActivityFormats.Gpx.Utils
{
    public static class GpxCoordinateUtils
    {
        public static List<LineString> CreateLinestringForGpxTrack(string fileName)
        {
            var gpx = GetGpxFromFile(fileName);

            return CreateLinestring(gpx);
        }

        public static List<LineString> CreateLinestring(gpxType gpx)
        {
            var result = new List<LineString>();

            if (gpx.trk != null)
            {
                foreach (var trk in gpx.trk)
                {
                    var coordinates = new List<List<double>>();

                    foreach (var seg in trk.trkseg)
                    {
                        foreach (var pt in seg.trkpt)
                        {
                            var coordinate = new List<double>();

                            coordinate.Add(Convert.ToDouble(pt.lat));
                            coordinate.Add(Convert.ToDouble(pt.lon));

                            coordinates.Add(coordinate);
                        }
                    }

                    if (coordinates.Count > 1)
                    {
                        var lineString = GeometryProducer.Instance.CreateLinestring(coordinates);
                        result.Add(lineString);
                    }
                }
            }

            return result;
        }

        public static gpxType GetGpxFromFile(string fileName)
        {
            try
            {
                return GetGarminGpxFromFile(fileName);
            }
            catch (Exception)
            {
                try
                {
                    gpxType gpx = null;
                    var xmlSerializer = new XmlSerializer(typeof(gpxType));

                    using (var stream = new FileStream(fileName, FileMode.Open))
                    {
                        gpx = (gpxType)xmlSerializer.Deserialize(stream);
                    }

                    return gpx;
                }
                catch (Exception)
                {
                    gpxType gpx = null;
                    var xmlSerializer = new XmlSerializer(typeof(gpxType));

                    var lastResortAttempt = File.ReadAllText(fileName).Replace("&", "&amp;");

                    using (var stream = new StringReader(lastResortAttempt))
                    {
                        gpx = (gpxType)xmlSerializer.Deserialize(stream);
                    }

                    return gpx;
                }
            }
        }

        public static gpxType GetGarminGpxFromFile(string fileName)
        {
            var gpx = new gpxType();
            var trkList = new List<trkType>();

            using (var stream = new FileStream(fileName, FileMode.Open))
            {
                using (var reader = new GpxReader(stream))
                {
                    while (reader.Read())
                    {
                        var trk = new trkType();
                        var seg = new trksegType();
                        var trkPtList = new List<wptType>();

                        switch (reader.ObjectType)
                        {
                            case GpxObjectType.Metadata:
                                break;
                            case GpxObjectType.WayPoint:
                                break;
                            case GpxObjectType.Route:
                                var route = reader.Route;

                                foreach (var routePoint in route.RoutePoints)
                                {
                                    foreach (var point in routePoint.RoutePoints)
                                    {

                                        trkPtList.Add(new wptType()
                                        {
                                            lat = Convert.ToDecimal(point.Latitude),
                                            lon = Convert.ToDecimal(point.Longitude)
                                        });
                                    }
                                }

                                break;
                            case GpxObjectType.Track:
                                var track = reader.Track;

                                foreach (var segment in track.Segments)
                                {
                                    foreach (var point in segment.TrackPoints)
                                    {
                                        trkPtList.Add(new wptType()
                                        {
                                            lat = Convert.ToDecimal(point.Latitude),
                                            lon = Convert.ToDecimal(point.Longitude)
                                        });
                                    }
                                }

                                break;
                        }

                        trk.trkseg = new trksegType[1] { seg };
                        seg.trkpt = trkPtList.ToArray();

                        trkList.Add(trk);
                    }
                }
            }

            gpx.trk = trkList.ToArray();

            return gpx;
        }

    }
}
