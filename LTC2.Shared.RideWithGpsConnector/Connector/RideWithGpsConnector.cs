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

        public async Task<Session> GetSession(string code)
        {
            var response = await _proxy.GetToken(new AuthorizeRequest { Code = code });

            var firstname = string.Empty;
            var lastname = string.Empty;

            if (!string.IsNullOrEmpty(response.User?.Name))
            {
                var spaceIndex = response.User.Name.IndexOf(' ');
                if (spaceIndex > 0)
                {
                    firstname = response.User.Name.Substring(0, spaceIndex);
                    lastname = response.User.Name.Substring(spaceIndex + 1);
                }
                else
                {
                    firstname = response.User.Name;
                }
            }

            var session = new Session
            {
                AccessToken = response.Access_token,
                RefreshToken = null,
                ExpiresAt = 0,
                Athlete = new Athlete
                {
                    Id = response.User?.Id ?? 0,
                    Firstname = firstname,
                    Lastname = lastname
                }
            };

            return session;
        }
    }
}
