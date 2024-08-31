namespace LTC2.Shared.Models.Dtos.SqlServer
{
    public class DtoTrackForAlltimePlace : DtoTrack
    {
        public DtoTrackForAlltimePlace(DtoTrack dtoTrack)
        {
            tracId = dtoTrack.tracId;
            tracExternalId = dtoTrack.tracExternalId;
            tracName = dtoTrack.tracName;
            tracAthleteId = dtoTrack.tracAthleteId;
            tracDate = dtoTrack.tracDate;
            tracTrack = dtoTrack.tracTrack;
            tracDistance = dtoTrack.tracDistance;
            tracPlaces = dtoTrack.tracPlaces;
        }

        public long alltId { get; set; }

        public string alltPlaceId { get; set; }
    }
}
