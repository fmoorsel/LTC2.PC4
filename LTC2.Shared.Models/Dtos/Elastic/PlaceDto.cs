namespace LTC2.Shared.Models.Dtos.Elastic
{
    public class PlaceDto
    {
        public string PlaceId { get; set; }

        public string Name { get; set; }

        public string FeaturePointer { get; set; }

        public BorderDto Border { get; set; }

        public BorderDto HitArea { get; set; }

    }
}
