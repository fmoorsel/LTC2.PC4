using LTC2.Shared.Models.Domain;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.StravaConnector.Exceptions;
using LTC2.Shared.StravaConnector.Interfaces;
using LTC2.Shared.StravaConnector.Models;
using LTC2.Shared.StravaConnector.Models.Requests;
using LTC2.Shared.StravaConnector.Models.Responses;
using LTC2.Shared.Utils.Generic;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LTC2.Shared.StravaConnector.Connector
{
    public class StravaConnector : IStravaConnector
    {
        private readonly ILogger<StravaConnector> _logger;
        private readonly GenericSettings _genericSettings;
        private readonly IStravaHttpProxy _stravaProxy;
        private readonly ISessionStore _sessionStore;

        private int _currentQuarterUsage;
        private int _currentDayUsage;

        private int _limitQuarterUsage;
        private int _limitDayUsage;

        public StravaConnector(
            ILogger<StravaConnector> logger,
            GenericSettings genericSettings,
            IStravaHttpProxy stravaProxy,
            ISessionStore sessionStore)
        {
            _logger = logger;
            _genericSettings = genericSettings;
            _stravaProxy = stravaProxy;
            _sessionStore = sessionStore;

            _currentQuarterUsage = -1;
            _currentDayUsage = -1;
            _limitQuarterUsage = -1;
            _limitDayUsage = -1;

        }

        public async Task<Session> GetSession(string code)
        {
            return await GetSession(code, AuthorizeType.AuthorizationCode);
        }

        public async Task<Session> GetSession(long athleteId)
        {
            var sessionFromStore = GetSessionFromStore(athleteId);

            return await GetSession(sessionFromStore);
        }

        public async Task<Session> GetSession(Session session)
        {
            if (IsValidSession(session))
            {
                return session;
            }
            else
            {
                var sessionFromStore = GetSessionFromStore(session.AthleteId, session);

                return await GetSession(sessionFromStore.RefreshToken, AuthorizeType.RefreshToken, session);
            }
        }

        private async Task<Session> GetSession(string code, AuthorizeType type, Session oldSession = null)
        {
            var authorizeRequest = new AuthorizeRequest()
            {
                Code = code,
                Type = type
            };

            var authorizeResponse = await _stravaProxy.GetToken(authorizeRequest);

            var session = new Session()
            {
                AccessToken = authorizeResponse.Access_token,
                RefreshToken = authorizeResponse.Refresh_token,
                ExpiresAt = authorizeResponse.Expires_at,
                Athlete = authorizeResponse.Athlete ?? oldSession?.Athlete
            };

            StoreSession(session);

            return session;
        }

        private Session GetSessionFromStore(long athleteId, Session currentSession = null)
        {
            return _sessionStore.Retrieve(athleteId, currentSession);
        }

        private bool IsValidSession(Session session)
        {
            var dateExpiresAt = FromUnixTime(Convert.ToInt64(session.ExpiresAt));
            var remainingTime = dateExpiresAt - DateTime.Now;

            return remainingTime.TotalSeconds > _genericSettings.SessionIsExpiringAfterSeconds;
        }

        private DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime).ToLocalTime();
        }

        private void StoreSession(Session session)
        {
            _sessionStore.Store(session);
        }

        public async Task<GetActivitiesResponse> GetActivities(GetActivitiesRequest request, string code)
        {
            var response = await _stravaProxy.GetActivities(request, code);

            return response;
        }

        public async Task BrowseActivities<TResultType>(
                GetActivitiesRequest request,
                string accessToken,
                TResultType subject,
                OnPreCheckActivity<TResultType> onPreCheckActivity,
                OnCheckActivity<TResultType> onCheckActivity,
                OnWaitingForSlot<TResultType> onWaitingForSlot) where TResultType : class
        {

            bool hasActivities = true;
            StravaActivity lastActivity = null;

            request.Page = 0;
            request.PerPage = 200;

            while (hasActivities)
            {
                request.Page++;

                var activities = await TryGetActivities(request, accessToken, lastActivity, onWaitingForSlot, subject);

                hasActivities = activities.Activities.Count > 0;

                if (hasActivities)
                {
                    foreach (var activity in activities.Activities)
                    {
                        if (!activity.IsManual)
                        {
                            try
                            {
                                var proximatedTrack = GeoCoder.DecodeToTrack(activity.Map?.SummaryPolyline);
                                var shouldCheck = onPreCheckActivity(activity, proximatedTrack, subject);

                                if (shouldCheck)
                                {
                                    var coordinateStreamRequest = new GetActivityCoordinateStreamRequest()
                                    {
                                        AthleteId = request.AthleteId,
                                        ActivityId = activity.Id,
                                        BypassCache = request.BypassCache
                                    };

                                    var coordinateStreamResponse = await TryGetActivityCoordinateStream(coordinateStreamRequest, accessToken, onWaitingForSlot, subject);
                                    var track = GetTrackFromResponse(coordinateStreamResponse);

                                    if (track.Count >= 2)
                                    {
                                        onCheckActivity(activity, track, subject);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, $"Unable to process activity {activity.Id} due to {ex.Message}");

                                await Task.Delay(1000);
                            }

                        }

                        lastActivity = activity;
                    }
                }
            }
        }

        private async Task<GetActivitiesResponse> TryGetActivities<TResultType>(GetActivitiesRequest request, string accessToken, StravaActivity lastActivity, OnWaitingForSlot<TResultType> onWaitingForSlot, TResultType subject) where TResultType : class
        {
            var shouldRetry = true;
            var alreadyRetried = false;

            while (shouldRetry)
            {
                try
                {
                    if (alreadyRetried)
                    {
                        _logger.LogDebug("Retry GetActivities, after quarter rate limit exceed.");
                    }

                    await TryGetQuarterSlot(onWaitingForSlot, subject);

                    var activities = await _stravaProxy.GetActivities(request, accessToken);

                    ActualizeUsage(activities);

                    return activities;
                }
                catch (StraveTooManyRequestsException ex)
                {
                    _logger.LogDebug(ex, "Too many requests reported by Strava and noticed by connector when retrieving activities.");


                    if (ex.Limits == null || !ex.Limits.HasLimits)
                    {
                        throw;
                    }

                    ActualizeUsage(ex.Limits);

                    if (ex.Limits.DayRateUsage >= ex.Limits.DayRateLimit)
                    {
                        // todo handle: in case daily clear queue and create new message based on last message
                        _logger.LogWarning($"Day limit exceeded on GetActivities {ex.Limits.DayRateUsage} {ex.Limits.DayRateLimit}");

                        throw new StravaTooManyDailyRequestsException(ex.Limits, ex);
                    }
                    else
                    {
                        shouldRetry = !alreadyRetried;

                        if (shouldRetry)
                        {
                            alreadyRetried = true;

                            // must be a quarter exceed, just wait until the next quarter starts and retry
                            _logger.LogWarning($"Quarter limit exceeded on GetActivities {ex.Limits.QuarterRateUsage} {ex.Limits.QuarterRateLimit}");

                            await WaitForQuarterSlot(onWaitingForSlot, subject);
                        }
                        else
                        {
                            return new GetActivitiesResponse($"{ex.Limits.QuarterRateLimit},{ex.Limits.DayRateLimit}", $"{ex.Limits.QuarterRateUsage},{ex.Limits.DayRateUsage}");
                        }
                    }
                }
            }

            return new GetActivitiesResponse();
        }

        public async Task<List<List<double>>> GetTrackForActivity<TResultType>(string activityId, bool bypassCache, string accessToken, OnWaitingForSlot<TResultType> onWaitingForSlot, TResultType subject) where TResultType : class
        {
            try
            {
                var coordinateStreamRequest = new GetActivityCoordinateStreamRequest()
                {
                    ActivityId = Int64.Parse(activityId),
                    BypassCache = bypassCache
                };

                var coordinateStramResponse = await TryGetActivityCoordinateStream(coordinateStreamRequest, accessToken, onWaitingForSlot, subject);
                var track = GetTrackFromResponse(coordinateStramResponse);

                if (track.Count >= 2)
                {
                    return track;
                }
            }
            catch (StravaTooManyDailyRequestsException)
            {
                // ignore for now, caller will use the non-precise track to get the last ride result (which is sort of ok)
            }

            return null;
        }

        private async Task<GetActivityCoordinateStreamResponse> TryGetActivityCoordinateStream<TResultType>(GetActivityCoordinateStreamRequest request, string accessToken, OnWaitingForSlot<TResultType> onWaitingForSlot, TResultType subject) where TResultType : class
        {
            var shouldRetry = true;
            var alreadyRetried = false;

            while (shouldRetry)
            {
                try
                {
                    if (alreadyRetried)
                    {
                        _logger.LogDebug("Retry GetActivityCoordinateStream, after quarter rate limit exceed.");
                    }

                    await TryGetQuarterSlot(onWaitingForSlot, subject);

                    var activities = await GetActivityCoordinateStream(request, accessToken);

                    ActualizeUsage(activities);

                    return activities;
                }
                catch (StraveTooManyRequestsException ex)
                {
                    _logger.LogDebug(ex, "Too many requests reported by Strava and noticed by connector when retrieving coordinates.");

                    if (ex.Limits == null || !ex.Limits.HasLimits)
                    {
                        throw;
                    }

                    ActualizeUsage(ex.Limits);

                    if (ex.Limits.DayRateUsage >= ex.Limits.DayRateLimit)
                    {
                        _logger.LogWarning($"Day limit exceeded on GetActivityCoordinateStream {ex.Limits.DayRateUsage} {ex.Limits.DayRateLimit}");

                        throw new StravaTooManyDailyRequestsException(ex.Limits, ex);
                    }
                    else
                    {
                        shouldRetry = !alreadyRetried;

                        if (shouldRetry)
                        {
                            alreadyRetried = true;

                            // must be a quarter exceed, just wait until the next quarter starts and retry
                            _logger.LogWarning($"Quarter limit exceeded on GetActivityCoordinateStream {ex.Limits.QuarterRateUsage} {ex.Limits.QuarterRateLimit}");

                            await WaitForQuarterSlot(onWaitingForSlot, subject);
                        }
                        else
                        {
                            return new GetActivityCoordinateStreamResponse($"{ex.Limits.QuarterRateLimit},{ex.Limits.DayRateLimit}", $"{ex.Limits.QuarterRateUsage},{ex.Limits.DayRateUsage}");
                        }
                    }
                }
            }

            return new GetActivityCoordinateStreamResponse();
        }


        private async Task TryGetQuarterSlot<TResultType>(OnWaitingForSlot<TResultType> onWaitingForSlot, TResultType subject) where TResultType : class
        {
            var limits = new LimitsOnlyResponse($"{_limitQuarterUsage},{_limitDayUsage}", $"{_currentQuarterUsage},{_currentDayUsage}");

            if (_currentQuarterUsage > 0 && _currentDayUsage > 0)
            {
                if (_currentDayUsage + 1 > _limitDayUsage)
                {
                    _logger.LogWarning($"About to exceed day limit exceeded, abort calculation {_currentQuarterUsage} {_limitDayUsage}");

                    throw new StravaTooManyDailyRequestsException(limits);
                }
                else if (_currentQuarterUsage + 1 > _limitQuarterUsage)
                {
                    _logger.LogWarning($"About to exceed quarter limit exceeded, wait for next quarter {_currentQuarterUsage} {_limitDayUsage}");

                    await WaitForQuarterSlot(onWaitingForSlot, subject);
                }
            }
        }

        private void ActualizeUsage(AbstractStravaResponse response)
        {
            if (response.HasLimits)
            {
                _limitDayUsage = response.DayRateLimit;
                _limitQuarterUsage = response.QuarterRateLimit;

                _currentQuarterUsage = response.QuarterRateUsage;
                _currentDayUsage = response.DayRateUsage;
            }
        }

        private List<List<double>> GetTrackFromResponse(GetActivityCoordinateStreamResponse response)
        {
            var result = new List<List<double>>();

            if (response.ActivityCoordinateStream?.Latlng?.Data != null)
            {
                return response.ActivityCoordinateStream.Latlng.Data.Select(d => new List<double>() { d[1], d[0] }).ToList();
            }

            return result;
        }

        public async Task<GetActivityCoordinateStreamResponse> GetActivityCoordinateStream(GetActivityCoordinateStreamRequest request, string accessToken)
        {
            var response = await _stravaProxy.GetActivityCoordinateStream(request, accessToken);

            return response;
        }

        private async Task WaitForQuarterSlot<TResultType>(OnWaitingForSlot<TResultType> onWaitingForSlot, TResultType subject) where TResultType : class
        {
            var currentStravaSlot = RoundUp(DateTime.UtcNow, TimeSpan.FromMinutes(15));

            var waitTime = currentStravaSlot - DateTime.UtcNow;
            var slackOnWaitInSeconds = 60;
            var slackOnWaitInMillSeconds = slackOnWaitInSeconds * 1000;

            if (waitTime.TotalMilliseconds > 0)
            {
                _logger.LogInformation($"Waiting for free Strava slot {waitTime.TotalMilliseconds}");

                onWaitingForSlot(currentStravaSlot.AddSeconds(slackOnWaitInSeconds).ToLocalTime(), subject);

                await Task.Delay(Convert.ToInt32(waitTime.TotalMilliseconds + slackOnWaitInSeconds));
            }
        }

        private DateTime RoundUp(DateTime dt, TimeSpan d)
        {
            return new DateTime((dt.Ticks + d.Ticks - 1) / d.Ticks * d.Ticks, dt.Kind);
        }
    }
}
