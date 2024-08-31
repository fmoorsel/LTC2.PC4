using LTC2.Shared.Models.Domain;

namespace LTC2.Shared.StravaConnector.Models.Responses
{
    public class AuthorizeResponse
    {

        public string Access_token { get; set; }

        public string Refresh_token { get; set; }

        public int Expires_in { get; set; }

        public int Expires_at { get; set; }

        public Athlete Athlete { get; set; }

    }
}
