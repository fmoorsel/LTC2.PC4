namespace LTC2.Shared.Models.Settings
{
    public class RideWithGpsHttpProxySettings : BaseHttpProxySettings
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public int MaxRoutesCount { get; set; } = 100;

        public double MinLat { get; set; } = double.MinValue;

        public double MaxLat { get; set; } = double.MaxValue;

        public double MinLon { get; set; } = double.MinValue;

        public double MaxLon { get; set; } = double.MaxValue;
    }
}
