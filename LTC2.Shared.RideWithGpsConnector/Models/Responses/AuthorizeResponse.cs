namespace LTC2.Shared.RideWithGpsConnector.Models.Responses
{
    public class AuthorizeResponse
    {
        public string Access_token { get; set; }
        public string Token_type { get; set; }
        public string Scope { get; set; }
        public long Created_at { get; set; }
        public long User_id { get; set; }
    }
}
