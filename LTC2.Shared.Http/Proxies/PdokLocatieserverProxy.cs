using LTC2.Shared.Http.Interfaces;
using LTC2.Shared.Http.Models.Responses;
using LTC2.Shared.Models.Settings;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LTC2.Shared.Http.Proxies
{
    public class PdokLocatieserverProxy : AbstractHttpProxy, IPdokLocatieserverProxy
    {
        public PdokLocatieserverProxy(
            ILogger<PdokLocatieserverProxy> logger,
            PdokHttpProxySettings settings) : base(logger, settings)
        {
        }

        /// <summary>
        /// Calls the PDOK Locatieserver /free endpoint.
        /// </summary>
        /// <param name="query">Address string to geocode (e.g. "Damrak 1 1012LG Amsterdam")</param>
        /// <param name="rows">Maximum number of results (default 1)</param>
        /// <param name="filterQuery">Optional Solr filter query (e.g. "type:adres")</param>
        public async Task<PdokFreeResponse> FreeAsync(string query, int rows = 1, string filterQuery = null)
        {
            var uri = $"/bzk/locatieserver/search/v3_1/free?q={Uri.EscapeDataString(query)}&rows={rows}";

            if (filterQuery != null)
                uri += $"&fq={Uri.EscapeDataString(filterQuery)}";

            return await ExecuteGetRequest<PdokFreeResponse>(uri);
        }
    }
}
