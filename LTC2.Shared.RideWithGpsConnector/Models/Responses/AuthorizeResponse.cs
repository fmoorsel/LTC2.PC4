namespace LTC2.Shared.RideWithGpsConnector.Models.Responses
{
    public class AuthorizeResponse
    {
        public string Access_token { get; set; }
        public RwGpsUser User { get; set; }
    }
}
