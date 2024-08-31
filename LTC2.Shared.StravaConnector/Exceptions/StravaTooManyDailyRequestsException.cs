using LTC2.Shared.StravaConnector.Models.Responses;
using System;

namespace LTC2.Shared.StravaConnector.Exceptions
{
    public class StravaTooManyDailyRequestsException : Exception
    {
        public LimitsOnlyResponse Limits { get; private set; }

        public StravaTooManyDailyRequestsException(LimitsOnlyResponse limits) : base("Too many daily requests")
        {
            Limits = limits;
        }

        public StravaTooManyDailyRequestsException(LimitsOnlyResponse limits, StraveTooManyRequestsException exception) : base("Too many daily requests", exception)
        {
            Limits = limits;
        }
    }
}
