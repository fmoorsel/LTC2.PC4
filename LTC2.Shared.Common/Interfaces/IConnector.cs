using LTC2.Shared.Common.Models;
using LTC2.Shared.Models.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LTC2.Shared.Common.Interfaces
{
    public delegate void OnCheckActivity<TResultType>(SourceActivity activity, List<List<double>> track, TResultType subject) where TResultType : class;
    public delegate bool OnPreCheckActivity<TResultType>(SourceActivity activity, List<List<double>> track, TResultType subject) where TResultType : class;
    public delegate void OnWaitingForSlot<TResultType>(DateTime waitUntil, TResultType subject) where TResultType : class;

    public interface IConnector
    {
        Task<Session> GetSession(string code);

        Task<Session> GetSession(long athleteId);

        Task<Session> GetSession(Session session);

        Task BrowseActivities<TResultType>(BrowseActivitiesRequest request, string accessToken, TResultType subject, OnPreCheckActivity<TResultType> onPreCheckActivity, OnCheckActivity<TResultType> onCheckActivity, OnWaitingForSlot<TResultType> onWaitingForSlot) where TResultType : class;

        Task<List<List<double>>> GetTrackForActivity<TResultType>(string activityId, bool bypassCache, string accessToken, OnWaitingForSlot<TResultType> onWaitingForSlot, TResultType subject) where TResultType : class;
    }

    public interface IConnector<TGetActivitiesRequest, TGetActivitiesResponse,
        TStreamRequest, TStreamResponse, TRoutesRequest, TRoutesResponse,
        TRouteGpxRequest, TRouteGpxResponse> : IConnector
    {
        public Task<TGetActivitiesResponse> GetActivities(TGetActivitiesRequest request, string code);

        public Task BrowseActivities<TResultType>(TGetActivitiesRequest request, string accessToken, TResultType subject, OnPreCheckActivity<TResultType> onPreCheckActivity, OnCheckActivity<TResultType> onCheckActivity, OnWaitingForSlot<TResultType> onWaitingForSlot) where TResultType : class;

        public Task<TStreamResponse> GetActivityCoordinateStream(TStreamRequest request, string accessToken);

        public Task<TRoutesResponse> GetRoutes(TRoutesRequest request);

        public Task<TRouteGpxResponse> GetRouteDetailsAsGpx(TRouteGpxRequest request);
    }
}
