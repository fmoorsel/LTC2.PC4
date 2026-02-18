using LTC2.Shared.Models.Domain;
using LTC2.Shared.RideWithGpsConnector.Interfaces;
using LTC2.Shared.RideWithGpsConnector.Models.Requests;
using LTC2.Shared.Stores.Interfaces;
using Microsoft.Extensions.Logging;
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
    }
}
