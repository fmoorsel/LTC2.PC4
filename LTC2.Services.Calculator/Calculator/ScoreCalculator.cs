using LTC2.Services.Calculator.Interfaces;
using LTC2.Services.Calculator.Models;
using LTC2.Services.Calculator.Services;
using LTC2.Shared.Models.Domain;
using LTC2.Shared.Models.Interprocess;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.Repositories.Interfaces;
using LTC2.Shared.StravaConnector.Exceptions;
using LTC2.Shared.StravaConnector.Interfaces;
using LTC2.Shared.StravaConnector.Models;
using LTC2.Shared.StravaConnector.Models.Requests;
using LTC2.Shared.Utils.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LTC2.Services.Calculator.Calculator
{
    public class ScoreCalculator : IScoreCalculator
    {
        private readonly CalculatorSettings _calculatorSettings;
        private readonly ILogger<ScoreCalculator> _logger;
        private readonly IMapRepository _mapRepository;
        private readonly IStravaConnector _stravaConnector;
        private readonly IScoresRepository _scoresRepository;
        private readonly StatusNotifier _statusNotifier;
        private readonly IIntermediateResultsRepository _intermediateResultsRepository;

        private readonly long _maxDuration = 100 * 60 * 60;
        private readonly long _maxDistance = 1600000;

        private readonly List<StravaActivityType> _activityTypes = new List<StravaActivityType>();

        public ScoreCalculator(
                GenericSettings genericSettings,
                CalculatorSettings calculatorSettings,
                IMapRepository mapRepository,
                IScoresRepository scoresRepository,
                IStravaConnector stravaConnector,
                IIntermediateResultsRepository intermediateResultsRepository,
                StatusNotifier statusNotifier,
                ILogger<ScoreCalculator> logger
            )
        {
            _calculatorSettings = calculatorSettings;
            _logger = logger;
            _mapRepository = mapRepository;
            _scoresRepository = scoresRepository;
            _intermediateResultsRepository = intermediateResultsRepository;
            _stravaConnector = stravaConnector;
            _statusNotifier = statusNotifier;

            ParseActivityTypes();
        }

        private void ParseActivityTypes()
        {
            if (_calculatorSettings.ActivityTypes != null)
            {
                foreach (var type in _calculatorSettings.ActivityTypes)
                {
                    try
                    {
                        var actType = (StravaActivityType)Enum.Parse(typeof(StravaActivityType), type);

                        _activityTypes.Add(actType);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Unable to add activity type: {type}, due to {ex.Message}");
                    }
                }
            }

            if (_activityTypes.Count == 0)
            {
                _activityTypes.Add(StravaActivityType.Ride);
            }
        }

        public async Task Calculate(CalculationJob job)
        {
            _logger.LogInformation($"Calclation job received for: {job.AthleteId} of type {job.Type}");

            var dayLimitDetect = false;
            var stravaSession = await GetSession(job);

            var request = new GetActivitiesRequest()
            {
                AthleteId = job.AthleteId,
                BypassCache = job.BypassCache
            };

            var calculationResult = new CalculationResult()
            {
                AthleteId = job.AthleteId,
                IsRefresh = job.Refresh
            };

            if (!job.Refresh && !job.IsRestoreInterMediate && !job.IsClearInterMediate)
            {
                var mostrecentVisitResult = await _scoresRepository.GetMostRecentVisit(job.AthleteId);

                if (mostrecentVisitResult != null)
                {
                    request.After = mostrecentVisitResult.VisitedOn.AddDays(-1);

                    _logger.LogInformation($"Getting recent activities only, form {request.After} and onwards.");

                    calculationResult = await _scoresRepository.GetMostRecentResult(job.AthleteId);

                    _logger.LogInformation($"Current result has {calculationResult.VisitedPlacesAllTime.Count} entries.");
                }
            }

            var start = DateTime.UtcNow;


            if (job.IsRestoreInterMediate)
            {
                calculationResult = _intermediateResultsRepository.GetIntermediateResult(job.AthleteId);

                job.Refresh = calculationResult.IsRefresh;
            }
            else
            {
                _intermediateResultsRepository.Clear(job.AthleteId);
            }

            if (!job.IsRestoreInterMediate && !job.IsClearInterMediate)
            {
                try
                {
                    await _stravaConnector.BrowseActivities(request, stravaSession.AccessToken, calculationResult, OnPreCheckActivity, OnCheckActivity, OnWaitingForSlot);
                }
                catch (StravaTooManyDailyRequestsException)
                {
                    _logger.LogWarning("Calculation aborted due to daily limit exceed, try again tomorrow (after 00:00UTC");

                    dayLimitDetect = true;
                }
            }

            if (!job.IsClearInterMediate && calculationResult.LastRideSample != null && calculationResult.LastRideSample.Updated)
            {
                var lastRide = calculationResult.LastRideSample;

                var preciseTrack = await _stravaConnector.GetTrackForActivity(calculationResult.LastRideSample.ExternalId, request.BypassCache, stravaSession.AccessToken, OnWaitingForSlot, calculationResult);

                lastRide.Track = preciseTrack ?? lastRide.Track;

                var placesLastRide = _mapRepository.CheckTrack(lastRide.Track);

                calculationResult.VisitedPlacesLastRide = new Dictionary<string, Visit>();

                foreach (var place in placesLastRide)
                {
                    var visit = new Visit()
                    {
                        PlaceId = place.Id,
                        VisitedOn = lastRide.VisitedOn,
                        Track = GeometryProducer.Instance.Simplify(lastRide.Track),
                        VisitedPlaces = placesLastRide.Select(place => place.Id).ToList(),
                        Place = place,
                        Name = lastRide.Name,
                        ExternalId = lastRide.ExternalId,
                        Distance = lastRide.Distance,
                        Updated = true
                    };

                    calculationResult.VisitedPlacesLastRide.Add(visit.PlaceId, visit);
                }
            }

            if (!job.IsClearInterMediate)
            {
                await _scoresRepository.StoreScores(job.Refresh, calculationResult);

                _intermediateResultsRepository.Clear(job.AthleteId);

                if (dayLimitDetect)
                {
                    NotifyLimit();
                }
                else
                {
                    NotifyResult((DateTime.UtcNow - start).TotalSeconds);
                }
            }
        }

        public void OnWaitingForSlot(DateTime waitUntil, CalculationResult subject)
        {
            _statusNotifier.SetNotification(StatusMessage.STATUS_WAIT, waitUntil.ToString());

            _intermediateResultsRepository.StoreIntermedidateResult(subject);
        }

        public void OnCheckActivity(StravaActivity activity, List<List<double>> track, CalculationResult subject)
        {
            _logger.LogDebug($"Check {activity.Name} {activity.DateTimeStart} {activity.Distance} {activity.Type}");

            var places = _mapRepository.CheckTrack(track);

            foreach (var place in places)
            {
                if (subject.VisitedPlacesAllTime.ContainsKey(place.Id))
                {
                    if (subject.VisitedPlacesAllTime[place.Id].VisitedOn > activity.DateTimeStart)
                    {
                        subject.VisitedPlacesAllTime[place.Id].VisitedOn = activity.DateTimeStart;
                        subject.VisitedPlacesAllTime[place.Id].Track = GeometryProducer.Instance.Simplify(track);
                        subject.VisitedPlacesAllTime[place.Id].VisitedPlaces = places.Select(place => place.Id).ToList();
                        subject.VisitedPlacesAllTime[place.Id].ExternalId = activity.Id.ToString();
                        subject.VisitedPlacesAllTime[place.Id].Distance = Convert.ToInt64(activity.Distance);
                        subject.VisitedPlacesAllTime[place.Id].Name = activity.Name;
                        subject.VisitedPlacesAllTime[place.Id].Updated = true;
                    }
                }
                else
                {
                    var visit = new Visit();

                    visit.PlaceId = place.Id;
                    visit.VisitedOn = activity.DateTimeStart;
                    visit.Track = GeometryProducer.Instance.Simplify(track);
                    visit.VisitedPlaces = places.Select(place => place.Id).ToList();
                    visit.Place = place;
                    visit.ExternalId = activity.Id.ToString();
                    visit.Distance = Convert.ToInt64(activity.Distance);
                    visit.Name = activity.Name;
                    visit.Updated = true;

                    subject.VisitedPlacesAllTime.Add(place.Id, visit);
                }

                if (activity.DateTimeStart.Year == DateTime.UtcNow.Year)
                {
                    if (subject.VisitedPlacesCurrentYear.ContainsKey(place.Id))
                    {
                        if (subject.VisitedPlacesCurrentYear[place.Id].VisitedOn > activity.DateTimeStart)
                        {
                            subject.VisitedPlacesCurrentYear[place.Id].VisitedOn = activity.DateTimeStart;
                            subject.VisitedPlacesCurrentYear[place.Id].Track = GeometryProducer.Instance.Simplify(track);
                            subject.VisitedPlacesCurrentYear[place.Id].VisitedPlaces = places.Select(place => place.Id).ToList();
                            subject.VisitedPlacesCurrentYear[place.Id].ExternalId = activity.Id.ToString();
                            subject.VisitedPlacesCurrentYear[place.Id].Distance = Convert.ToInt64(activity.Distance);
                            subject.VisitedPlacesCurrentYear[place.Id].Name = activity.Name;
                            subject.VisitedPlacesCurrentYear[place.Id].Updated = true;
                        }
                    }
                    else
                    {
                        var visit = new Visit();

                        visit.PlaceId = place.Id;
                        visit.VisitedOn = activity.DateTimeStart;
                        visit.Track = GeometryProducer.Instance.Simplify(track);
                        visit.VisitedPlaces = places.Select(place => place.Id).ToList();
                        visit.Place = place;
                        visit.ExternalId = activity.Id.ToString();
                        visit.Distance = Convert.ToInt64(activity.Distance);
                        visit.Name = activity.Name;
                        visit.Updated = true;

                        subject.VisitedPlacesCurrentYear.Add(place.Id, visit);
                    }
                }
            }
        }

        private void NotifyLimit()
        {
            _logger.LogWarning("Calculation aborted due to daily limit exceed, try again tomorrow (after 00:00UTC");

            var tryAgainTime = DateTime.UtcNow.Date.AddDays(1).AddMinutes(1).ToLocalTime();

            _statusNotifier.SetNotification(StatusMessage.STATUS_LIMIT, tryAgainTime.ToString());
        }

        private void NotifyResult(double seconds)
        {
            _logger.LogInformation($"Result calculated in: {seconds} seconds");

            _statusNotifier.SetNotification(StatusMessage.STATUS_RESULT, $"calculated in: {seconds} seconds");
        }

        private void NotifyActivityCheck(StravaActivity activity)
        {
            _logger.LogDebug($"Precheck {activity.Name} {activity.DateTimeStart} {activity.Distance} {activity.Type}");

            _statusNotifier.SetNotification(StatusMessage.STATUS_CHECK, $"{activity.DateTimeStart} {activity.Name}");
        }

        private bool IsWhiteListedActivity(long activityId)
        {
            if (_calculatorSettings.BlackListedActivities != null)
            {
                return _calculatorSettings.WhiteListedActivities.Contains(activityId.ToString());
            }

            return false;
        }

        private bool IsBlackListedActivity(long activityId)
        {
            if (_calculatorSettings.BlackListedActivities != null)
            {
                return _calculatorSettings.BlackListedActivities.Contains(activityId.ToString());
            }

            return false;
        }

        public bool OnPreCheckActivity(StravaActivity activity, List<List<double>> track, CalculationResult subject)
        {
            var whiteListed = IsWhiteListedActivity(activity.Id);
            var notExcedingElapsedTime = whiteListed || (activity.ElapsedTime <= _maxDuration);
            var notExcedingDistance = whiteListed || activity.Distance <= _maxDistance;
            var blackListed = IsBlackListedActivity(activity.Id);

            var result = _activityTypes.Contains(activity.Type) && !activity.IsManual && notExcedingDistance && notExcedingElapsedTime && !blackListed;

            NotifyActivityCheck(activity);

            if (result && track.Count > 1)
            {
                subject.ProgressCount++;

                if (_calculatorSettings.IntermediateResultAfterCount > 0 && subject.ProgressCount % _calculatorSettings.IntermediateResultAfterCount == 0)
                {
                    _intermediateResultsRepository.StoreIntermedidateResult(subject);
                }

                var places = _mapRepository.PreCheckTrack(track);

                var newPlacesCount = places.Where(p => IsPlaceRelevant(activity, p, subject)).Count();

                if (places.Count > 0 && (subject.LastRideSample == null || activity.DateTimeStart > subject.LastRideSample.VisitedOn))
                {
                    var visit = new Visit();

                    visit.VisitedOn = activity.DateTimeStart;
                    visit.Track = track;
                    visit.ExternalId = activity.Id.ToString();
                    visit.Distance = Convert.ToInt64(activity.Distance);
                    visit.Name = activity.Name;
                    visit.Updated = true;

                    subject.VisitedPlacesLastRide = new Dictionary<string, Visit>() { { "temp", visit } };
                }

                return newPlacesCount > 0;
            }

            return result;
        }

        private bool IsPlaceRelevant(StravaActivity activity, Place place, CalculationResult subject)
        {
            var currentYear = DateTime.Now.Year;

            var isNewAlltime = !subject.VisitedPlacesAllTime.ContainsKey(place.Id);
            var isNewForCurrentYear = activity.DateTimeStart.Year == currentYear && !subject.VisitedPlacesCurrentYear.ContainsKey(place.Id);

            return isNewAlltime || isNewForCurrentYear;
        }

        public void Init()
        {
            _logger.LogInformation("Start initialisation of score calculator");

            var places = _mapRepository.GetAllPlaces();

            _logger.LogInformation($"Score calculator initialised: {places.Count} places");
        }

        private async Task<Session> GetSession(CalculationJob job)
        {
            if (job.Code != null)
            {
                return await _stravaConnector.GetSession(job.Code);
            }
            else if (job.Session != null)
            {
                return await _stravaConnector.GetSession(job.Session);
            }
            else
            {
                return await _stravaConnector.GetSession(job.AthleteId);
            }
        }
    }
}
