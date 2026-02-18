using LTC2.Shared.Http.Proxies;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.RideWithGpsConnector.Interfaces;
using LTC2.Shared.RideWithGpsConnector.Models.Requests;
using LTC2.Shared.RideWithGpsConnector.Models.Responses;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LTC2.Shared.RideWithGpsConnector.Proxies
{
    public class RideWithGpsHttpProxy : AbstractHttpProxy, IRideWithGpsHttpProxy
    {
        private readonly ILogger<RideWithGpsHttpProxy> _logger;
        private readonly RideWithGpsHttpProxySettings _settings;

        public RideWithGpsHttpProxy(
            ILogger<RideWithGpsHttpProxy> logger,
            RideWithGpsHttpProxySettings settings) : base(logger, settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public async Task<AuthorizeResponse> GetToken(AuthorizeRequest request)
        {
            var parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>("client_id", _settings.ClientId));
            parameters.Add(new KeyValuePair<string, string>("client_secret", _settings.ClientSecret));
            parameters.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
            parameters.Add(new KeyValuePair<string, string>("code", request.Code));
            return await ExecuteFormUrlEncodedRequest<AuthorizeResponse>("/oauth/token", null, parameters);
        }
    }
}
