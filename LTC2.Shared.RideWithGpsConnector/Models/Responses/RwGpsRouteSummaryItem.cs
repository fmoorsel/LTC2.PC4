using System;

namespace LTC2.Shared.RideWithGpsConnector.Models.Responses
{
    internal class RwGpsRouteSummaryItem
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public double Distance { get; set; }
        public DateTime? Created_at { get; set; }
        public DateTime? Updated_at { get; set; }
    }
}
