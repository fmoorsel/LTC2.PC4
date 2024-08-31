namespace LTC2.Shared.Models.Domain
{
    public enum CalculationType { bike = 1, multi = 2, foot = 3, all = 4 }

    public class CalculationJob
    {

        public long AthleteId { get; set; }

        public bool IsRestoreInterMediate { get; set; }

        public bool IsClearInterMediate { get; set; }

        public bool Refresh { get; set; }

        public bool BypassCache { get; set; }

        public CalculationType Type { get; set; }

        public string Code { get; set; }

        public Session Session { get; set; }

    }
}
