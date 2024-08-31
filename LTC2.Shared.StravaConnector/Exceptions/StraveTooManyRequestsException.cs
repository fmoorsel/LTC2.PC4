using LTC2.Shared.Http.Exceptions;
using LTC2.Shared.StravaConnector.Models.Responses;
using System;

namespace LTC2.Shared.StravaConnector.Exceptions
{
    public class StraveTooManyRequestsException : Exception
    {
        public LimitsOnlyResponse Limits { get; private set; }

        public StraveTooManyRequestsException(LimitsOnlyResponse limits, HttpProxyException hpException) : base("Too many requests", hpException)
        {
            Limits = limits;
        }
    }
}
