using LTC2.Shared.Http.Exceptions;
using LTC2.Shared.Http.Interfaces;
using LTC2.Shared.Models.Settings;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LTC2.Shared.Http.Proxies
{
    public class LTC2HttpProxy : AbstractHttpProxy, ILTC2HttpProxy
    {
        public LTC2HttpProxy(ILogger<AbstractHttpProxy> logger, LTC2HttpProxySettings proxySettings) : base(logger, proxySettings)
        {
        }

        public async Task Update(string accessToken, bool refresh, bool byPassCache, bool isRestore, bool isClear)
        {
            var authHeader = new AuthenticationHeaderValue("Bearer", accessToken);
            var uri = $"/api/Update/update?refresh={refresh}&bypassCache={byPassCache}&isRestore={isRestore}&isClear={isClear}";

            await ExecutePostRequest(uri, authHeader);
        }

        public async Task<bool> HasIntermediateResult(string accessToken)
        {
            var authHeader = new AuthenticationHeaderValue("Bearer", accessToken);
            var uri = $"/api/Profile/intermediateresult";

            var content = await ExecuteGetRequest(uri, authHeader);

            if (content == null)
            {
                throw new InvalidValueException($"Content should be 'true' or 'false' but was null.");
            }
            else if (content.ToLower() == "true")
            {
                return true;
            }
            else if (content.ToLower() == "false")
            {
                return false;
            }
            else
            {
                throw new InvalidValueException($"Content should be 'true' or 'false' but was '{content}'.");
            }
        }

    }
}
