using System;
using System.Collections.Generic;
using System.Linq;

namespace LTC2.Shared.Models.Domain
{
    public class CalculationResult
    {
        public long AthleteId { get; set; }

        public bool IsRefresh { get; set; }

        public int ProgressCount { get; set; }

        public Dictionary<string, Visit> VisitedPlacesAllTime { get; set; } = new Dictionary<string, Visit>();

        public List<Visit> UpdatedPlacesAllTime
        {
            get
            {
                return VisitedPlacesAllTime.Values.Where(v => v.Updated).ToList();
            }
        }

        public Dictionary<string, Visit> VisitedPlacesCurrentYear { get; set; } = new Dictionary<string, Visit>();

        public List<Visit> UpdatedPlacesCurrentYear
        {
            get
            {
                return VisitedPlacesCurrentYear.Values.Where(v => v.Updated).ToList();
            }
        }

        public Dictionary<string, Visit> VisitedPlacesLastRide { get; set; } = new Dictionary<string, Visit>();


        public Visit LastRideSample
        {
            get
            {
                if (VisitedPlacesLastRide.Count > 0)
                {
                    return VisitedPlacesLastRide.Values.First();
                }

                return null;
            }
        }

        public List<Visit> UpdatedPlacesLastRide
        {
            get
            {
                return VisitedPlacesLastRide.Values.Where(v => v.Updated).ToList();
            }
        }

        public DateTime? DateLastRide
        {
            get
            {
                if (VisitedPlacesLastRide.Count > 0)
                {
                    var aVisit = VisitedPlacesLastRide.Values.First();

                    return aVisit.VisitedOn;
                }
                else
                {
                    return default(DateTime);
                }
            }
        }

        public List<Track> UpdatedTracks
        {
            get
            {
                return GetTracks(true);
            }
        }

        public List<Track> Tracks
        {
            get
            {
                return GetTracks(false);
            }
        }


        private List<Track> GetTracks(bool onlyUpdated)
        {
            var tracks = new List<Track>();

            var resultSet = new Dictionary<string, Track>();

            foreach (var visit in VisitedPlacesAllTime.Values)
            {
                if (!resultSet.ContainsKey(visit.ExternalId) && (!onlyUpdated || visit.Updated))
                {
                    if (visit.Track.Count > 1)
                    {
                        var track = new Track(visit);

                        resultSet.Add(visit.ExternalId, track);
                    }
                }
            }

            foreach (var visit in VisitedPlacesCurrentYear.Values)
            {
                if (!resultSet.ContainsKey(visit.ExternalId) && (!onlyUpdated || visit.Updated))
                {
                    if (visit.Track.Count > 1)
                    {
                        var track = new Track(visit);

                        resultSet.Add(visit.ExternalId, track);
                    }
                }
            }

            foreach (var visit in VisitedPlacesLastRide.Values)
            {
                if (!resultSet.ContainsKey(visit.ExternalId) && (!onlyUpdated || visit.Updated))
                {
                    if (visit.Track.Count > 1)
                    {
                        var track = new Track(visit);

                        resultSet.Add(visit.ExternalId, track);
                    }
                }
            }

            tracks.AddRange(resultSet.Values);

            return tracks;

        }
    }
}
