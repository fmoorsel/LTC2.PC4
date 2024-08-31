using Newtonsoft.Json;
using System;

namespace LTC2.Shared.StravaConnector.Models
{
    public class StravaActivity
    {
        public long Id { get; set; }

        public string Name { get; set; }

        [JsonProperty("type")]
        private string _type { get; set; }

        [JsonIgnore]
        public StravaActivityType Type
        {
            get
            {
                return (StravaActivityType)Enum.Parse(typeof(StravaActivityType), _type);
            }
        }

        public double Distance { get; set; }

        [JsonProperty("moving_time")]
        public int MovingTime { get; set; }

        [JsonProperty("elapsed_time")]
        public int ElapsedTime { get; set; }

        [JsonProperty("start_date")]
        public string StartDate { get; set; }

        [JsonProperty("start_date_local")]
        public string StartDateLocal { get; set; }

        [JsonProperty("manual")]
        public bool IsManual { get; set; }

        public DateTime DateTimeStart
        {
            get { return DateTime.Parse(StartDate); }
        }

        public DateTime DateTimeStartLocal
        {
            get { return DateTime.Parse(StartDateLocal); }
        }

        public TimeSpan MovingTimeSpan
        {
            get { return TimeSpan.FromSeconds(MovingTime); }
        }

        public TimeSpan ElapsedTimeSpan
        {
            get { return TimeSpan.FromSeconds(ElapsedTime); }
        }

        public string Timezone { get; set; }

        public StravaMap Map { get; set; }

    }
}
