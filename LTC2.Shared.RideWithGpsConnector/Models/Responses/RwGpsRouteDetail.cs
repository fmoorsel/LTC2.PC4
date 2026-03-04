using System.Collections.Generic;

namespace LTC2.Shared.RideWithGpsConnector.Models.Responses
{
    internal class RwGpsRouteDetail
    {
        public string Name { get; set; }
        public List<RwGpsRouteTrackPoint> Track_points { get; set; }
    }
}
