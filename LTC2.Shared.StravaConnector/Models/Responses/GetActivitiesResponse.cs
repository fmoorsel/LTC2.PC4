using System.Collections.Generic;

namespace LTC2.Shared.StravaConnector.Models.Responses
{
    public class GetActivitiesResponse : AbstractStravaResponse
    {
        public GetActivitiesResponse() : base()
        {
        }

        public GetActivitiesResponse(string limits, string usage) : base(limits, usage)
        {
        }

        public List<StravaActivity> Activities { get; set; } = new List<StravaActivity>();
    }
}
