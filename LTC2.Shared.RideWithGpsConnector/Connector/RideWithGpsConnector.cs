using LTC2.Shared.Common.Interfaces;
using LTC2.Shared.Common.Models;
using LTC2.Shared.Models.Domain;
using LTC2.Shared.RideWithGpsConnector.Interfaces;
using LTC2.Shared.RideWithGpsConnector.Models.Requests;
using LTC2.Shared.RideWithGpsConnector.Models.Responses;
using LTC2.Shared.Stores.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LTC2.Shared.RideWithGpsConnector.Connector
{
    public class RideWithGpsConnector : IRideWithGpsConnector
    {
        private readonly ILogger<RideWithGpsConnector> _logger;
        private readonly IRideWithGpsHttpProxy _proxy;
        private readonly ISessionStore _sessionStore;

        public RideWithGpsConnector(
            ILogger<RideWithGpsConnector> logger,
            IRideWithGpsHttpProxy proxy,
            ISessionStore sessionStore)
        {
            _logger = logger;
            _proxy = proxy;
            _sessionStore = sessionStore;
        }

        public Task<Session> GetSession(string code)
        {
            throw new NotSupportedException("Use GetSession(string code, string redirectUri) for RideWithGps.");
        }

        public async Task<Session> GetSession(string code, string redirectUri)
        {
            var tokenResponse = await _proxy.GetToken(new AuthorizeRequest { Code = code, RedirectUri = redirectUri });
            var userResponse = await _proxy.GetCurrentUser(tokenResponse.Access_token);

            var user = userResponse?.User;

            var session = new Session
            {
                AccessToken = tokenResponse.Access_token,
                RefreshToken = null,
                ExpiresAt = 0,
                Origin = Session.RideWithGpsSession,
                Athlete = new Athlete
                {
                    Id = user?.Id ?? 0,
                    Firstname = user?.Name ?? string.Empty,
                    Lastname = string.Empty
                }
            };

            _sessionStore.Store(session);

            return session;
        }

        public Task<Session> GetSession(long athleteId)
        {
            return Task.FromResult(_sessionStore.Retrieve(athleteId, Session.RideWithGpsSession));
        }

        public Task<Session> GetSession(Session session)
        {
            return Task.FromResult(session);
        }

        public async Task<List<RwGpsSyncItem>> GetActivities(GetActivitiesRequest request, string accessToken)
        {
            return await _proxy.GetActivities(request, accessToken);
        }

        public async Task<List<List<double>>> GetTrackForActivity(string activityId, bool bypassCache, string accessToken, OnWaitingForSlot onWaitingForSlot, CalculationResult subject)
        {
            var trip = await _proxy.GetTrip(long.Parse(activityId), bypassCache, accessToken);
            var track = trip?.Coordinates;

            if (track != null && track.Count >= 2)
            {
                return track;
            }

            return null;
        }

        public async Task BrowseActivities(
            BrowseActivitiesRequest request,
            string accessToken,
            CalculationResult subject,
            OnPreCheckActivity onPreCheckActivity,
            OnCheckActivity onCheckActivity,
            OnWaitingForSlot onWaitingForSlot)
        {
            var rwgpsRequest = new GetActivitiesRequest
            {
                BypassCache = request.BypassCache,
                After = request.After ?? new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            var syncItems = await _proxy.GetActivities(rwgpsRequest, accessToken);

            if (subject is CalculationResult calculationResult)
            {
                calculationResult.SkipPreCheckPlaces = true;
            }

            foreach (var item in syncItems)
            {
                try
                {
                    var trip = await _proxy.GetTrip(item.Item_id, request.BypassCache, accessToken);
                    if (trip == null || trip.Stationary)
                        continue;

                    var track = trip.Coordinates ?? new List<List<double>>();

                    var sourceActivity = new SourceActivity
                    {
                        Id = item.Item_id,
                        Name = trip.Name,
                        ActivityType = trip.ActivityType,
                        Distance = trip.Distance,
                        ElapsedTime = (long)trip.Duration,
                        IsManual = false,
                        DateTimeStart = trip.StartTime ?? DateTime.MinValue,
                        Source = ActivitySource.RideWithGps
                    };

                    var shouldCheck = onPreCheckActivity(sourceActivity, track, subject);

                    if (shouldCheck && track.Count >= 2)
                    {
                        onCheckActivity(sourceActivity, track, subject);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Unable to process trip {item.Item_id} due to {ex.Message}");
                }
            }
        }
    }
}
