using System;
using System.Collections.Generic;

namespace LTC2.Shared.RideWithGpsConnector.Models.Responses
{
    public class RwGpsTrip
    {
        public string Name { get; set; }
        public string ActivityType { get; set; }
        public double Distance { get; set; }
        public DateTime? StartTime { get; set; }
        public double MovingTime { get; set; }
        public List<List<double>> Coordinates { get; set; }
    }
}
