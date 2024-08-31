using LTC2.Shared.Models.Domain;
using LTC2.Shared.Repositories.Interfaces;
using LTC2.Webapps.MainApp.Models;
using LTC2.Webapps.MainApp.Models.Requests;
using LTC2.Webapps.MainApp.Services;
using LTC2.Webapps.MainApp.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace LTC2.Webapps.MainApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly TokenUtils _tokenUtils;
        private readonly IScoresRepository _scoreRepository;
        private readonly IInternalProfileRepository _internalProfileRepository;
        private readonly IIntermediateResultsRepository _intermediateResultsRepository;
        private readonly ProfileManager _profileManager;
        private readonly AppSettings _appSettings;

        public ProfileController(
            TokenUtils tokenUtils,
            IScoresRepository scoreRepository,
            IIntermediateResultsRepository intermediateResultsRepository,
            IInternalProfileRepository internalProfileRepository,
            ProfileManager profileManager,
            AppSettings appSettings)
        {
            _tokenUtils = tokenUtils;
            _scoreRepository = scoreRepository;
            _internalProfileRepository = internalProfileRepository;
            _intermediateResultsRepository = intermediateResultsRepository;
            _profileManager = profileManager;
            _appSettings = appSettings;
        }

        [HttpGet]
        [Authorize]
        [Route("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var authHeader = _tokenUtils.GetAuthenticationHeader(HttpContext.Request);
            var token = authHeader?.Parameter;

            if (token != null)
            {
                if (_tokenUtils.ValidateToken(token))
                {
                    var profile = _tokenUtils.GetProfileFormToken(token);
                    var athleteId = Convert.ToInt64(profile.AthleteId);

                    var score = await _scoreRepository.GetMostRecentResult(athleteId);

                    profile.PlacesInAllTimeScore = score.VisitedPlacesAllTime.Values.Select(p => new ProfileVisit(p, false)).ToList();
                    profile.PlacesInYearScore = score.VisitedPlacesCurrentYear.Values.Select(p => new ProfileVisit(p, false)).ToList();
                    profile.PlacesInLastRideScore = score.VisitedPlacesLastRide.Values.Select(p => new ProfileVisit(p, true)).ToList();

                    profile.TrackLastRide = score.LastRideSample?.Track ?? new List<List<double>>();

                    profile.ClientId = _profileManager.CurrentProfile;

                    if (!_appSettings.IsStandAlone)
                    {
                        var internalProfile = await _internalProfileRepository.GetInternalProfile(Convert.ToInt64(profile.AthleteId));

                        if (internalProfile.AthleteId == athleteId)
                        {
                            profile.Email = internalProfile.Email;
                        }
                    }

                    return Ok(profile);
                }
            }

            return Unauthorized();
        }

        [HttpGet]
        [Authorize]
        [Route("intermediateresult")]
        public IActionResult HasIntermediateResult()
        {
            var authHeader = _tokenUtils.GetAuthenticationHeader(HttpContext.Request);
            var token = authHeader?.Parameter;

            if (token != null)
            {
                if (_tokenUtils.ValidateToken(token))
                {
                    var profile = _tokenUtils.GetProfileFormToken(token);
                    var athleteId = Convert.ToInt64(profile.AthleteId);

                    var hasIntermediateResult = _intermediateResultsRepository.HasIntermediateResult(athleteId);

                    return Ok(hasIntermediateResult);
                }
            }

            return Unauthorized();

        }



        [HttpGet]
        [Authorize]
        [Route("profile/track/alltime/{placeId}")]
        public async Task<IActionResult> GetAlltimeTrack([FromRoute] string placeId, [FromQuery] bool detailed = false)
        {
            var authHeader = _tokenUtils.GetAuthenticationHeader(HttpContext.Request);
            var token = authHeader?.Parameter;

            if (token != null)
            {
                if (_tokenUtils.ValidateToken(token))
                {
                    var profile = _tokenUtils.GetProfileFormToken(token);
                    var athleteId = Convert.ToInt64(profile.AthleteId);

                    var track = await _scoreRepository.GetAlltimeTrackForPlace(athleteId, placeId, detailed | _appSettings.ForceDetailed);

                    if (track == null)
                    {
                        return NotFound();
                    }

                    return Ok(track);
                }
            }

            return Unauthorized();
        }

        [HttpGet]
        [Authorize]
        [Route("profile/track/alltime")]
        public async Task<IActionResult> GetAlltimeTracks()
        {
            var authHeader = _tokenUtils.GetAuthenticationHeader(HttpContext.Request);
            var token = authHeader?.Parameter;

            if (token != null)
            {
                if (_tokenUtils.ValidateToken(token))
                {
                    var profile = _tokenUtils.GetProfileFormToken(token);
                    var athleteId = Convert.ToInt64(profile.AthleteId);

                    var tracks = await _scoreRepository.GetAlltimeTracksForAllPlaces(athleteId);

                    if (tracks == null)
                    {
                        return NotFound();
                    }

                    return Ok(tracks);
                }
            }

            return Unauthorized();
        }

        [HttpPost]
        [Authorize]
        [Route("profile/email")]
        public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailRequest request)
        {
            var authHeader = _tokenUtils.GetAuthenticationHeader(HttpContext.Request);
            var token = authHeader?.Parameter;

            if (token != null && !_appSettings.IsStandAlone)
            {
                if (_tokenUtils.ValidateToken(token))
                {
                    if (IsValidEmail(request.Email))
                    {
                        var profile = _tokenUtils.GetProfileFormToken(token);
                        var athleteId = Convert.ToInt64(profile.AthleteId);

                        var internalProfile = new InternalProfile()
                        {
                            AthleteId = athleteId,
                            Email = request.Email
                        };

                        await _internalProfileRepository.UpsertInternalProfile(internalProfile);
                    }
                    else
                    {
                        return BadRequest("Invalid email address");
                    }

                    return Ok();
                }
            }

            return Unauthorized();
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var mailAddres = new MailAddress(email);

                return mailAddres.Address == email;
            }
            catch
            {
                return false;
            }
        }

        [HttpGet]
        [Route("token")]
        public IActionResult GetTokenFromCookie()
        {
            var tokenCookie = HttpContext.Request.Cookies["token"];

            if (!string.IsNullOrEmpty(tokenCookie))
            {
                var validUntil = _tokenUtils.TokenIsValidUntil(tokenCookie);

                if (validUntil >= DateTime.UtcNow.AddHours(1))
                {
                    return Ok(tokenCookie);
                }
            }

            return Unauthorized();
        }
    }
}
