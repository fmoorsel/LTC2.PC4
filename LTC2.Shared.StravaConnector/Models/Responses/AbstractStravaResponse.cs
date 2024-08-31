using System;

namespace LTC2.Shared.StravaConnector.Models.Responses
{
    public abstract class AbstractStravaResponse
    {
        public AbstractStravaResponse()
        {

        }

        public AbstractStravaResponse(string limits, string usage)
        {
            try
            {
                if (limits != null)
                {
                    limits = limits.Trim();

                    var rateLimits = limits.Split(',');

                    QuarterRateLimit = Int32.Parse(rateLimits[0]);
                    DayRateLimit = Int32.Parse(rateLimits[1]);
                }

                if (usage != null)
                {
                    usage = usage.Trim();

                    var rateUsage = usage.Split(',');

                    QuarterRateUsage = Int32.Parse(rateUsage[0]);
                    DayRateUsage = Int32.Parse(rateUsage[1]);
                }

                HasLimits = (limits != null) && (usage != null);
            }
            catch (Exception)
            {
            }
        }

        public bool HasLimits { get; set; } = false;

        public int QuarterRateLimit { get; set; } = int.MaxValue;

        public int QuarterRateUsage { get; set; } = int.MaxValue;

        public int DayRateLimit { get; set; } = int.MaxValue;

        public int DayRateUsage { get; set; } = int.MaxValue;
    }
}
