using System.Collections.Generic;

namespace LTC2.Shared.Models.Responses
{
    public class GetProfileResponse
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
        public string Id { get; set; }

        public string Date { get; set; }

        public string ScriptId
        {
            get
            {
                return '"' + Id + '"';
            }
        }

    }
}
