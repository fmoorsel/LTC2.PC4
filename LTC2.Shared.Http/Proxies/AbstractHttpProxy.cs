using LTC2.Shared.Http.Exceptions;
using LTC2.Shared.Http.Pool;
using LTC2.Shared.Models.Settings;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LTC2.Shared.Http.Proxies
{
    public abstract class AbstractHttpProxy
    {
        private readonly ILogger<AbstractHttpProxy> _logger;
        private readonly BaseHttpProxySettings _proxySettings;

        public static MediaTypeHeaderValue ApplicationJsonMediaTypeHeaderValue = MediaTypeHeaderValue.Parse("application/json");
        public static MediaTypeWithQualityHeaderValue ApplicationJsonMediaTypeWithQualityHeaderValue = MediaTypeWithQualityHeaderValue.Parse("application/json");

        private HttpClient _httpClient;

        public AbstractHttpProxy(ILogger<AbstractHttpProxy> logger, BaseHttpProxySettings proxySettings)
        {
            _logger = logger;
            _proxySettings = proxySettings;

            _httpClient = HttpClientPool.GetInstance().GetHttpClient(_proxySettings.Url, _proxySettings.TimeOut, _proxySettings.Pooled);
        }

        protected async Task<TResponse> ExecuteGetRequest<TResponse>(string uri, AuthenticationHeaderValue authHeader = null)
            where TResponse : class
        {
            var response = await ExecuteHttpRequest<TResponse>(HttpMethod.Get, uri, authHeader);
            return response;
        }

        protected async Task<TResponse> ExecuteGetRequest<TResponse>(string uri, Dictionary<string, string> filteredResponseHeaders, AuthenticationHeaderValue authHeader = null)
            where TResponse : class
        {
            var response = await ExecuteHttpRequest<TResponse>(HttpMethod.Get, uri, authHeader, filteredResponseHeaders);
            return response;
        }

        protected async Task<TResponse> ExecuteGetRequest<TRequest, TResponse>(string uri, TRequest request, AuthenticationHeaderValue authHeader = null)
            where TRequest : class
            where TResponse : class
        {
            if (request != null)
            {
                var properties = request.GetType().GetProperties()
                    .Where(x => x.CanRead)
                    .Where(x => x.GetValue(request, null) != null)
                    .ToDictionary(x => x.Name, x => x.GetValue(request, null));

                var propertyNames = properties
                    .Where(x => !(x.Value is string) && x.Value is IEnumerable)
                    .Select(x => x.Key)
                    .ToList();

                foreach (var key in propertyNames)
                {
                    var valueType = properties[key].GetType();
                    var valueElemType = valueType.IsGenericType
                                            ? valueType.GetGenericArguments()[0]
                                            : valueType.GetElementType();
                    if (valueElemType.IsPrimitive || valueElemType == typeof(string))
                    {
                        var enumerable = properties[key] as IEnumerable;
                        properties[key] = string.Join(",", enumerable.Cast<object>());
                    }
                }

                var parameters = string.Join("&", properties
                    .Select(x => string.Concat(
                    Uri.EscapeDataString(x.Key), "=",
                    Uri.EscapeDataString(x.Value.ToString()))));

                uri = $"{uri}?{parameters}";
            }

            var response = await ExecuteHttpRequest<TResponse>(HttpMethod.Get, uri, authHeader);
            return response;
        }

        protected async Task<string> ExecuteGetRequest(string uri, AuthenticationHeaderValue authHeader = null)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            requestMessage.Headers.Accept.Add(ApplicationJsonMediaTypeWithQualityHeaderValue);

            if (authHeader != null)
            {
                requestMessage.Headers.Authorization = authHeader;
            }

            var responseMessage = await _httpClient.SendAsync(requestMessage);

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new HttpProxyException((int)responseMessage.StatusCode);
            }

            var responseContent = responseMessage.Content;
            var responseBody = await responseContent.ReadAsStringAsync();

            return responseBody;
        }

        protected async Task<TResponse> ExecutePutRequest<TResponse>(string uri, AuthenticationHeaderValue authHeader = null)
            where TResponse : class
        {
            var response = await ExecuteHttpRequest<TResponse>(HttpMethod.Put, uri, authHeader);
            return response;
        }

        protected async Task<TResponse> ExecuteDeleteRequest<TResponse>(string uri, AuthenticationHeaderValue authHeader = null)
            where TResponse : class
        {
            var response = await ExecuteHttpRequest<TResponse>(HttpMethod.Delete, uri, authHeader);
            return response;
        }

        protected async Task<TResponse> ExecuteHttpRequest<TResponse>(HttpMethod method, string uri, AuthenticationHeaderValue authHeader = null, Dictionary<string, string> filteredResponseHeaders = null)
            where TResponse : class
        {
            var requestMessage = new HttpRequestMessage(method, uri);
            requestMessage.Headers.Accept.Add(ApplicationJsonMediaTypeWithQualityHeaderValue);

            if (authHeader != null)
            {
                requestMessage.Headers.Authorization = authHeader;
            }

            var responseMessage = await _httpClient.SendAsync(requestMessage);
            var responseContent = responseMessage.Content;
            var responseBody = await responseContent.ReadAsStringAsync();

            if (filteredResponseHeaders != null)
            {
                var headers = responseMessage.Headers;

                foreach (var header in filteredResponseHeaders)
                {
                    if (headers.Contains(header.Key))
                    {
                        filteredResponseHeaders[header.Key] = headers.GetValues(header.Key).FirstOrDefault();
                    }
                }
            }

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new HttpProxyException((int)responseMessage.StatusCode, filteredResponseHeaders);
            }

            var result = JsonConvert.DeserializeObject<TResponse>(responseBody);

            return result;
        }

        protected async Task ExecuteHttpRequest(HttpMethod method, string uri, AuthenticationHeaderValue authHeader = null)
        {
            var requestMessage = new HttpRequestMessage(method, uri);
            requestMessage.Headers.Accept.Add(ApplicationJsonMediaTypeWithQualityHeaderValue);

            if (authHeader != null)
            {
                requestMessage.Headers.Authorization = authHeader;
            }

            var responseMessage = await _httpClient.SendAsync(requestMessage);

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new HttpProxyException((int)responseMessage.StatusCode);
            }
        }

        protected async Task<TResponse> ExecutePostRequest<TRequest, TResponse>(string uri, TRequest request, AuthenticationHeaderValue authHeader = null)
            where TRequest : class
            where TResponse : class
        {
            var response = await ExecutePostOrPutRequest<TRequest, TResponse>(HttpMethod.Post, uri, request, authHeader);
            return response;
        }

        protected async Task<TResponse> ExecutePutRequest<TRequest, TResponse>(string uri, TRequest request, AuthenticationHeaderValue authHeader = null)
            where TRequest : class
            where TResponse : class
        {
            var response = await ExecutePostOrPutRequest<TRequest, TResponse>(HttpMethod.Put, uri, request, authHeader);
            return response;
        }

        private async Task<TResponse> ExecutePostOrPutRequest<TRequest, TResponse>(HttpMethod method, string uri, TRequest request, AuthenticationHeaderValue authHeader = null)
            where TRequest : class
            where TResponse : class
        {
            var requestBody = JsonConvert.SerializeObject(request);
            var requestContent = new StringContent(requestBody);
            requestContent.Headers.ContentType = ApplicationJsonMediaTypeHeaderValue;

            var requestMessage = new HttpRequestMessage(method, uri);
            requestMessage.Headers.Accept.Add(ApplicationJsonMediaTypeWithQualityHeaderValue);
            requestMessage.Content = requestContent;

            if (authHeader != null)
            {
                requestMessage.Headers.Authorization = authHeader;
            }

            var response = await _httpClient.SendAsync(requestMessage);
            var responseContent = response.Content;
            var responseBody = await responseContent.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpProxyException((int)response.StatusCode);
            }

            var result = JsonConvert.DeserializeObject<TResponse>(responseBody);
            return result;
        }


        protected async Task ExecutePostRequest(string uri, AuthenticationHeaderValue authHeader = null)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);

            if (authHeader != null)
            {
                requestMessage.Headers.Authorization = authHeader;
            }

            var response = await _httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpProxyException((int)response.StatusCode);
            }
        }


        protected async Task<TResponse> ExecuteFormUrlEncodedRequest<TResponse>(string uri, Dictionary<string, string> headers, IEnumerable<KeyValuePair<string, string>> requestData)
            where TResponse : class
        {
            var nameValueCollection = requestData.ToList();

            var requestContent = new FormUrlEncodedContent(nameValueCollection);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
            requestMessage.Headers.Accept.Add(ApplicationJsonMediaTypeWithQualityHeaderValue);

            if (headers != null && headers.Count > 0)
            {
                foreach (var header in headers)
                {
                    if (!requestMessage.Headers.Contains(header.Key))
                    {
                        requestMessage.Headers.Add(header.Key, header.Value);
                    }
                }
            }

            requestMessage.Content = requestContent;

            var response = await _httpClient.SendAsync(requestMessage);
            var responseContent = response.Content;
            var responseBody = await responseContent.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpProxyException((int)response.StatusCode);
            }

            var result = JsonConvert.DeserializeObject<TResponse>(responseBody);
            return result;
        }


        #region IDisposable Support

        private bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue)
            {
                return;
            }

            if (disposing)
            {
                // Only dispose the http client instance when the pool is disabled
                if (_proxySettings != null && !_proxySettings.Pooled)
                {
                    _httpClient.Dispose();
                }

                _httpClient = null;
            }

            disposedValue = true;
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
