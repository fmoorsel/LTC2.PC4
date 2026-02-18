using LTC2.Shared.Models.Domain;
using LTC2.Shared.RideWithGpsConnector.Interfaces;
using LTC2.Shared.RideWithGpsConnector.Models.Requests;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace LTC2.Shared.RideWithGpsConnector.Connector
{
    public class RideWithGpsConnector : IRideWithGpsConnector
    {
        private readonly ILogger<RideWithGpsConnector> _logger;
        private readonly IRideWithGpsHttpProxy _proxy;

        public RideWithGpsConnector(
            ILogger<RideWithGpsConnector> logger,
            IRideWithGpsHttpProxy proxy)
        {
            _logger = logger;
            _proxy = proxy;
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
                Athlete = new Athlete
                {
                    Id = user?.Id ?? 0,
                    Firstname = user?.Name ?? string.Empty,
                    Lastname = string.Empty
                }
            };

            return session;
        }
    }
}
