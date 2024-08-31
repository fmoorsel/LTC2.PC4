using LTC2.Shared.ActivityFormats.Tcx.Models.Generated;
using LTC2.Shared.Utils.Utils;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace LTC2.Shared.ActivityFormats.Tcx.Utils
{
    public static class TcxCoordinateUtils
    {
        public static List<LineString> CreateLinestringForTcxTrack(string fileName)
        {
            var tcx = GetTcxFromFile(fileName);

            return CreateLinestring(tcx);
        }

        public static List<LineString> CreateLinestring(TrainingCenterDatabase_t tcx)
        {
            var result = new List<LineString>();

            if (tcx.Activities != null && tcx.Activities.Activity != null && tcx.Activities.Activity.Length >= 1)
            {
                var activity = tcx.Activities.Activity[0];
                var coordinates = new List<List<double>>();

                foreach (var lap in activity.Lap)
                {
                    var lapTrack = lap.Track;

                    if (lapTrack != null)
                    {
                        foreach (var track in lapTrack)
                        {
                            foreach (var point in track.Trackpoint)
                            {
                                if (point.Position != null)
                                {
                                    var coordinate = new List<double>();

                                    coordinate.Add(Convert.ToDouble(point.Position.LatitudeDegrees));
                                    coordinate.Add(Convert.ToDouble(point.Position.LongitudeDegrees));

                                    coordinates.Add(coordinate);
                                }
                            }
                        }
                    }
                }

                if (coordinates.Count > 1)
                {
                    var lineString = GeometryProducer.Instance.CreateLinestring(coordinates);
                    result.Add(lineString);
                }
            }

            return result;
        }
        public static TrainingCenterDatabase_t GetTcxFromFile(string fileName)
        {
            try
            {
                TrainingCenterDatabase_t tcx = null;
                var xmlSerializer = new XmlSerializer(typeof(TrainingCenterDatabase_t));

                var contents = File.ReadAllText(fileName).Trim();

                using (var stream = new StringReader(contents))
                {
                    tcx = (TrainingCenterDatabase_t)xmlSerializer.Deserialize(stream);
                }

                return tcx;
            }
            catch (Exception)
            {
                TrainingCenterDatabase_t tcx = null;
                var xmlSerializer = new XmlSerializer(typeof(TrainingCenterDatabase_t));

                var lastResortAttempt = File.ReadAllText(fileName).Replace("&", "&amp;").Trim();

                using (var stream = new StringReader(lastResortAttempt))
                {
                    tcx = (TrainingCenterDatabase_t)xmlSerializer.Deserialize(stream);
                }

                return tcx;
            }

        }
    }
}
