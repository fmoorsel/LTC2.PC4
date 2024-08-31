using System.Collections.Generic;

namespace LTC2.Shared.Models.Domain
{
    public class Profile
    {
        public string Name { get; set; }

        public string AthleteId { get; set; }

        public string Email { get; set; }

        public string ClientId { get; set; }

        public List<ProfileVisit> PlacesInAllTimeScore { get; set; } = new List<ProfileVisit>();

        public List<ProfileVisit> PlacesInYearScore { get; set; } = new List<ProfileVisit>();

        public List<ProfileVisit> PlacesInLastRideScore { get; set; } = new List<ProfileVisit>();

        public List<List<double>> TrackLastRide { get; set; } = new List<List<double>>();
    }

    public class ProfileVisit
    {
        private readonly Visit _visit;
        private readonly bool _longTimestamp;

        public ProfileVisit(Visit visit, bool longTimestamp)
        {
            _visit = visit;
            _longTimestamp = longTimestamp;
        }

        public string Id { get { return _visit.PlaceId; } }

        public string Date { get { return _longTimestamp ? _visit.VisitedOn.ToString("yyyy-MM-dd hh:mm:ss") : _visit.VisitedOn.ToString("yyyy-MM-dd"); } }

    }
}
