namespace LTC2.Shared.Models.Settings
{
    public class AuthorizationSettings
    {
        public string Key { get; set; }

        public string ValidIssuers { get; set; }

        public string ValidAudience { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }
    }
}
