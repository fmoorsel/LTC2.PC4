using System;
using System.Collections.Generic;

namespace LTC2.Shared.RideWithGpsConnector.Models.Responses
{
    internal class RwGpsTripDetail
    {
        public string Activity_type { get; set; }
        public double Distance { get; set; }
        public DateTime? Departed_at { get; set; }
        public double Moving_time { get; set; }
        public List<RwGpsTrackPoint> Track_points { get; set; }
    }
}
