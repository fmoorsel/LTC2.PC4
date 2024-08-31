namespace LTC2.Shared.StravaConnector.Models.Requests
{
    public enum AuthorizeType
    {
        AuthorizationCode = 10,
        RefreshToken = 11

    }

    public class AuthorizeRequest
    {
        public AuthorizeType Type { get; set; }

        public string Code { get; set; }
    }
}
