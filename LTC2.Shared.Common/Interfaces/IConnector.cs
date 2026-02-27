using LTC2.Shared.Common.Models;
using LTC2.Shared.Models.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LTC2.Shared.Common.Interfaces
{
    public delegate void OnCheckActivity(SourceActivity activity, List<List<double>> track, CalculationResult subject);
    public delegate bool OnPreCheckActivity(SourceActivity activity, List<List<double>> track, CalculationResult subject);
    public delegate void OnWaitingForSlot(DateTime waitUntil, CalculationResult subject);

    public interface IConnector
    {
        Task<Session> GetSession(string code, string redirectUri);

        Task<Session> GetSession(long athleteId);

        Task<Session> GetSession(Session session);

        Task BrowseActivities(BrowseActivitiesRequest request, string accessToken, CalculationResult subject, OnPreCheckActivity onPreCheckActivity, OnCheckActivity onCheckActivity, OnWaitingForSlot onWaitingForSlot);

        Task<List<List<double>>> GetTrackForActivity(string activityId, bool bypassCache, string accessToken, OnWaitingForSlot onWaitingForSlot, CalculationResult subject);
    }
}
