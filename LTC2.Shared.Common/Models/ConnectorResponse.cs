namespace LTC2.Shared.Common.Models
{
    public class ConnectorResponse
    {
        public bool LimitsExceeded { get; set; }

        public bool HasLimits { get; set; }

        public int QuarterRateLimit { get; set; } = int.MaxValue;

        public int QuarterRateUsage { get; set; } = int.MaxValue;

        public int DayRateLimit { get; set; } = int.MaxValue;

        public int DayRateUsage { get; set; } = int.MaxValue;
    }
}
