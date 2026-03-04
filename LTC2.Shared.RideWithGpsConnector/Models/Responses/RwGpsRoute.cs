using System;

namespace LTC2.Shared.RideWithGpsConnector.Models.Responses
{
    public class RwGpsRoute
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public double Distance { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
