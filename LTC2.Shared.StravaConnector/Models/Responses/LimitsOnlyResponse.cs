namespace LTC2.Shared.StravaConnector.Models.Responses
{
    public class LimitsOnlyResponse : AbstractStravaResponse
    {
        public LimitsOnlyResponse(string limits, string usage) : base(limits, usage)
        {
        }
    }
}
