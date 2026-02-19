using LTC2.Shared.Common.Interfaces;
using LTC2.Shared.Models.Domain;
using LTC2.Shared.RideWithGpsConnector.Models.Requests;
using LTC2.Shared.RideWithGpsConnector.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LTC2.Shared.RideWithGpsConnector.Interfaces
{
    public interface IRideWithGpsConnector
    {
        Task<Session> GetSession(string code, string redirectUri);
        Task<Session> GetSession(long athleteId);
        Task<Session> GetSession(Session session);
        Task<List<RwGpsSyncItem>> GetActivities(GetActivitiesRequest request, string accessToken);
        Task BrowseActivities<TResultType>(GetActivitiesRequest request, string accessToken, TResultType subject, OnPreCheckActivity<RwGpsTrip, TResultType> onPreCheckActivity, OnCheckActivity<RwGpsTrip, TResultType> onCheckActivity, OnWaitingForSlot<TResultType> onWaitingForSlot) where TResultType : class;
        Task<List<List<double>>> GetTrackForActivity<TResultType>(string activityId, bool bypassCache, string accessToken, OnWaitingForSlot<TResultType> onWaitingForSlot, TResultType subject) where TResultType : class;
    }
}
