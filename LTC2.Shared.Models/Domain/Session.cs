using System.Text.Json.Serialization;

namespace LTC2.Shared.Models.Domain
{
    public class Session
    {
        public const string StravaSession = "s";
        public const string RideWithGpsSession = "r";

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public long ExpiresAt { get; set; }

        public string Origin { get; set; }

        public Athlete Athlete { get; set; }


        [JsonIgnore]
        public long AthleteId
        {
            get
            {
                return Athlete == null ? -1 : Athlete.Id;
            }
        }
    }
}
