using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using ProjNet.CoordinateSystems;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LTC2.Shared.Utils.Utils
{
    public class GeometryProducer
    {
        public static GeometryProducer Instance = new GeometryProducer();

        public GeometryProducer()
        {

        }
        public LineString CreateLinestring(List<List<double>> track)
        {
            var coordinates = track.Select(c => new Coordinate(c[0], c[1])).ToArray();

            return GetFactory().CreateLineString(coordinates);
        }

        public LineString SimplifyToLineString(List<List<double>> track)
        {
            var simpleCoordinates = Simplify(track);
            var coordinates = simpleCoordinates.Select(c => new Coordinate(c[0], c[1])).ToArray();

            return GetFactory().CreateLineString(coordinates);
        }

        public List<List<double>> Simplify(List<List<double>> track)
        {
            var coordinates = track.Select(c => new Coordinate(c[0], c[1])).ToArray();

            var simpl = new NetTopologySuite.Simplify.VWLineSimplifier(coordinates, 0.0003);
            var simpleTrack = simpl.Simplify();

            var simpleCoordinates = simpleTrack.Select(c => new List<double>() { c[0], c[1] }).ToList();

            return simpleCoordinates;
        }

        public string SimplifyToLinestringAsWktString(List<List<double>> track)
        {
            var wktWriter = new WKTWriter();

            return wktWriter.Write(SimplifyToLineString(track));
        }

        public string CreateLinestringAsWktString(List<List<double>> track)
        {
            var wktWriter = new WKTWriter();

            return wktWriter.Write(CreateLinestring(track));
        }

        public List<List<double>> GetTrackFromWktLineString(string wktString)
        {
            var wktReader = new WKTReader(GetGeometryServices());

            if (wktString != null)
            {
                var linestring = wktReader.Read(wktString) as LineString;

                if (linestring != null)
                {
                    return linestring.Coordinates.Select(c => new List<double>() { c[0], c[1] }).ToList();
                }
            }

            return new List<List<double>>();
        }

        public GeometryFactory GetFactory()
        {
            var service = GetGeometryServices();

            return service.CreateGeometryFactory();
        }

        private NtsGeometryServices GetGeometryServices()
        {
            var wgs84 = GeographicCoordinateSystem.WGS84 as CoordinateSystem;

            return new NtsGeometryServices(new PrecisionModel(), Convert.ToInt32(wgs84.AuthorityCode));
        }

    }
}
