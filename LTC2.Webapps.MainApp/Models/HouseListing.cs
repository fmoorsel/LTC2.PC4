using System.Collections.Generic;

namespace LTC2.Webapps.MainApp.Models
{
    public class HouseListing
    {
        public long? GlobalId { get; set; }
        public long? TinyId { get; set; }
        public string Url { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string Postcode { get; set; }
        public string Neighbourhood { get; set; }
        public string Municipality { get; set; }

        public long? Price { get; set; }
        public string PriceFormatted { get; set; }
        public string Status { get; set; }
        public string OfferingType { get; set; }

        public int? Bedrooms { get; set; }
        public int? Rooms { get; set; }
        public int? LivingArea { get; set; }
        public int? PlotArea { get; set; }
        public string EnergyLabel { get; set; }
        public string ObjectType { get; set; }
        public string HouseType { get; set; }
        public int? ConstructionYear { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string PublicationDate { get; set; }
        public List<string> PhotoUrls { get; set; }

        public bool HasGarden { get; set; }
        public bool HasBalcony { get; set; }
        public bool HasRoofTerrace { get; set; }
        public bool HasSolarPanels { get; set; }
        public bool HasHeatPump { get; set; }
        public bool IsMonument { get; set; }
        public bool IsAuction { get; set; }
    }
}
