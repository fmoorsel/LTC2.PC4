using LTC2.Shared.Common.Interfaces;
using LTC2.Shared.Models.Domain;
using LTC2.Shared.StravaConnector.Models;
using LTC2.Shared.StravaConnector.Models.Requests;
using LTC2.Shared.StravaConnector.Models.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LTC2.Shared.StravaConnector.Interfaces
{
    public delegate void OnCheckActivity<TResultType>(StravaActivity activity, List<List<double>> track, TResultType subject) where TResultType : class;
    public delegate bool OnPreCheckActivity<TResultType>(StravaActivity activity, List<List<double>> track, TResultType subject) where TResultType : class;
    public delegate void OnWaitingForSlot<TResultType>(DateTime waitUntil, TResultType subject) where TResultType : class;

    public interface IStravaConnector : IConnector<StravaActivity, GetActivitiesRequest, GetActivitiesResponse,
        GetActivityCoordinateStreamRequest, GetActivityCoordinateStreamResponse,
        GetRoutesRequest, GetRoutesResponse,
        GetRouteDetailsAsGpxRequest, GetRouteDetailsAsGpxReponse>
    {
        // Strava-specific overloads of BrowseActivities and GetTrackForActivity that use
        // the Strava-specific (single type parameter) delegate variants for backward compatibility.
        public Task BrowseActivities<TResultType>(GetActivitiesRequest request, string accessToken, TResultType subject, OnPreCheckActivity<TResultType> onPreCheckActivity, OnCheckActivity<TResultType> onCheckActivity, OnWaitingForSlot<TResultType> onWaitingForSlot) where TResultType : class;

        public Task<List<List<double>>> GetTrackForActivity<TResultType>(string activityId, bool bypassCache, string accessToken, OnWaitingForSlot<TResultType> onWaitingForSlot, TResultType subject) where TResultType : class;
    }
}
