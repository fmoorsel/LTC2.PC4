namespace LTC2.Shared.Models.Dtos.Elastic
{
    public class ActivityDto
    {
        public string Id { get; set; }

        public string ExternalId { get; set; }

        public string Name { get; set; }

        public string AthleteId { get; set; }

        public TrackDto Track { get; set; }
    }
}
