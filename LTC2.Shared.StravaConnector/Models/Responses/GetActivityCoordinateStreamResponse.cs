namespace LTC2.Shared.StravaConnector.Models.Responses
{
    public class GetActivityCoordinateStreamResponse : AbstractStravaResponse
    {
        public GetActivityCoordinateStreamResponse() : base()
        {

        }

        public GetActivityCoordinateStreamResponse(string limits, string usage) : base(limits, usage)
        {
        }

        public StravaActivityCoordinateStream ActivityCoordinateStream { get; set; }
    }
}
