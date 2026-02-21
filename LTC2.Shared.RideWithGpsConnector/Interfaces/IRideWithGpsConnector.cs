using LTC2.Shared.Common.Interfaces;
using LTC2.Shared.Models.Domain;
using LTC2.Shared.RideWithGpsConnector.Models.Requests;
using LTC2.Shared.RideWithGpsConnector.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LTC2.Shared.RideWithGpsConnector.Interfaces
{
    public interface IRideWithGpsConnector : IConnector
    {
        Task<Session> GetSession(string code, string redirectUri);
        Task<List<RwGpsSyncItem>> GetActivities(GetActivitiesRequest request, string accessToken);
        Task BrowseActivities<TResultType>(GetActivitiesRequest request, string accessToken, TResultType subject, OnPreCheckActivity<TResultType> onPreCheckActivity, OnCheckActivity<TResultType> onCheckActivity, OnWaitingForSlot<TResultType> onWaitingForSlot) where TResultType : class;
    }
}
