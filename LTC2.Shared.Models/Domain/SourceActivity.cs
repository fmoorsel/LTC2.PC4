using System;

namespace LTC2.Shared.Models.Domain
{
    public class SourceActivity
    {
        public long Id { get; set; }

        public long ElapsedTime { get; set; }

        public string ActivityType { get; set; }

        public double Distance { get; set; }

        public bool IsManual { get; set; }

        public DateTime DateTimeStart { get; set; }

        public string Name { get; set; }

        public ActivitySource Source { get; set; }
    }
}
