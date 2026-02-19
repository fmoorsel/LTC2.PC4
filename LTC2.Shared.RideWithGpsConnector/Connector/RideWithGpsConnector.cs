using LTC2.Shared.Common.Interfaces;
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
            return Task.FromResult(_sessionStore.Retrieve(athleteId));
        }

        public Task<Session> GetSession(Session session)
        {
            return Task.FromResult(session);
        }

        public async Task<List<RwGpsSyncItem>> GetActivities(GetActivitiesRequest request, string accessToken)
        {
            return await _proxy.GetActivities(request, accessToken);
        }

        public async Task<List<List<double>>> GetTrackForActivity<TResultType>(string activityId, bool bypassCache, string accessToken, OnWaitingForSlot<TResultType> onWaitingForSlot, TResultType subject) where TResultType : class
        {
            var trip = await _proxy.GetTrip(long.Parse(activityId), bypassCache, accessToken);
            var track = trip?.Coordinates;

            if (track != null && track.Count >= 2)
            {
                return track;
            }

            return null;
        }

        public async Task BrowseActivities<TResultType>(
            GetActivitiesRequest request,
            string accessToken,
            TResultType subject,
            OnPreCheckActivity<RwGpsTrip, TResultType> onPreCheckActivity,
            OnCheckActivity<RwGpsTrip, TResultType> onCheckActivity,
            OnWaitingForSlot<TResultType> onWaitingForSlot) where TResultType : class
        {
            var syncItems = await _proxy.GetActivities(request, accessToken);

            foreach (var item in syncItems)
            {
                try
                {
                    var trip = await _proxy.GetTrip(item.Item_id, request.BypassCache, accessToken);
                    if (trip == null)
                        continue;

                    var track = trip.Coordinates ?? new List<List<double>>();
                    var shouldCheck = onPreCheckActivity(trip, track, subject);

                    if (shouldCheck && track.Count >= 2)
                    {
                        onCheckActivity(trip, track, subject);
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
