using Dynastream.Fit;
using LTC2.Shared.Utils.Utils;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LTC2.Shared.ActivityFormats.Fit.Utils
{
    public static class FitCoordinateUtils
    {
        public static List<LineString> CreateLinestringForForTrack(string fileName)
        {
            var result = new List<LineString>();

            using (var fitStream = new FileStream(fileName, FileMode.Open))
            {
                var decoder = new Decode();

                var fitListener = new FitListener();
                decoder.MesgEvent += fitListener.OnMesg;

                decoder.Read(fitStream);

                var fitMessages = fitListener.FitMessages;
                var coordinates = new List<List<double>>();

                foreach (RecordMesg mesg in fitMessages.RecordMesgs)
                {
                    var latLong = GetLatLong(mesg);

                    if (latLong != null)
                    {
                        coordinates.Add(latLong);
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


        public static List<double> GetLatLong(RecordMesg mesg)
        {
            var lat = GetFieldWithOverrides(mesg, RecordMesg.FieldDefNum.PositionLat);
            var lng = GetFieldWithOverrides(mesg, RecordMesg.FieldDefNum.PositionLong);

            if (lat.HasValue && lng.HasValue)
            {
                return new List<double> { Convert.ToDouble(lat.Value) / 11930465, Convert.ToDouble(lng.Value) / 11930465 };
            }

            return null;
        }

        private static int? GetFieldWithOverrides(Mesg mesg, byte fieldNumber)
        {
            var profileField = Profile.GetField(mesg.Num, fieldNumber);

            if (profileField == null)
            {
                return null;
            }

            var fields = mesg.GetOverrideField(fieldNumber).ToList();

            if (fields.Count == 1)
            {
                return fields[0].GetValue() as int?;
            }
            else
            {
                return null;
            }
        }

    }
}
