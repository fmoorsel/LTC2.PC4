using LTC2.Shared.Models.Domain;
using NetTopologySuite.Geometries;

namespace LTC2.Shared.Models.Dtos.Memory
{
    public class PlacePolygon : Polygon
    {
        public PlacePolygon(LinearRing shell) : base(shell)
        {
        }

        public PlacePolygon(LinearRing shell, LinearRing[] holes) : base(shell, holes)
        {
        }

        public PlacePolygon(LinearRing shell, GeometryFactory factory) : base(shell, factory)
        {
        }

        public PlacePolygon(LinearRing shell, LinearRing[] holes, GeometryFactory factory) : base(shell, holes, factory)
        {
        }

        public PlacePolygon(LinearRing shell, LinearRing[] holes, Place place) : base(shell, holes)
        {
            Place = place;
        }

        public Place Place { get; set; }
    }
}
