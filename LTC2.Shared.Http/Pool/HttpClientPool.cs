using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace LTC2.Shared.Http.Pool
{
    public class HttpClientPool
    {
        private static HttpClientPool _instance;

        private Dictionary<string, HttpClient> _httpClients;

        private HttpClientPool()
        {
            _httpClients = new Dictionary<string, HttpClient>();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }

        public HttpClient GetHttpClient(string baseUrl, int timeoutInMS, bool httpClientPoolEnabled)
        {
            if (httpClientPoolEnabled)
            {
                lock (_httpClients)
                {
                    if (!_httpClients.ContainsKey(baseUrl))
                    {
                        var httpClient = CreateHttpClient(baseUrl, timeoutInMS);

                        _httpClients.Add(baseUrl, httpClient);
                    }
                }

                return _httpClients[baseUrl];
            }
            else
            {
                var httpClient = CreateHttpClient(baseUrl, timeoutInMS);
                return httpClient;
            }
        }

        private HttpClient CreateHttpClient(string baseUrl, int timeoutInMS)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(baseUrl);

            if (timeoutInMS != -1)
            {
                httpClient.Timeout = TimeSpan.FromMilliseconds(timeoutInMS);
            }

            return httpClient;
        }

        public static HttpClientPool GetInstance()
        {
            if (_instance == null)
            {
                _instance = new HttpClientPool();
            }

            return _instance;
        }
    }
}
