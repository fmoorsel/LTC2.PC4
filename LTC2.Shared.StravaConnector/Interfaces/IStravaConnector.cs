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

    public interface IStravaConnector
    {
        public Task<Session> GetSession(string code);

        public Task<Session> GetSession(long athleteId);

        public Task<Session> GetSession(Session session);

        public Task<GetActivitiesResponse> GetActivities(GetActivitiesRequest request, string code);

        public Task BrowseActivities<TResultType>(GetActivitiesRequest request, string accessToken, TResultType subject, OnPreCheckActivity<TResultType> onPreCheckActivity, OnCheckActivity<TResultType> onCheckActivity, OnWaitingForSlot<TResultType> onWaitingForSlot) where TResultType : class;

        public Task<GetActivityCoordinateStreamResponse> GetActivityCoordinateStream(GetActivityCoordinateStreamRequest request, string accessToken);

        public Task<List<List<double>>> GetTrackForActivity<TResultType>(string activityId, bool bypassCache, string accessToken, OnWaitingForSlot<TResultType> onWaitingForSlot, TResultType subject) where TResultType : class;


    }
}
