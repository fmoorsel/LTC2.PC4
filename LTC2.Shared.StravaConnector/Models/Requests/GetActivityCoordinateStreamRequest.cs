namespace LTC2.Shared.StravaConnector.Models.Requests
{
    public class GetActivityCoordinateStreamRequest
    {
        public long AthleteId { get; set; }

        public long ActivityId { get; set; }

        public bool BypassCache { get; set; }
    }
}
