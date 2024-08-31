using Newtonsoft.Json;

namespace LTC2.Shared.Models.Domain
{
    public class Athlete
    {
        public long Id { get; set; }

        public string Username { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        [JsonIgnore]
        public string Name
        {
            get
            {
                return $"{Firstname} {Lastname}";
            }
        }

    }
}
