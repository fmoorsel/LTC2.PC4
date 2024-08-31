using System;
using System.Collections.Generic;

namespace LTC2.Shared.Http.Exceptions
{
    public class HttpProxyException : Exception
    {
        public int Code { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public HttpProxyException(int code) : base($"Http Proxy Exception: {code}")
        {
            Code = code;
        }

        public HttpProxyException(int code, Dictionary<string, string> headers) : base($"Http Proxy Exception: {code}")
        {
            Code = code;
            Headers = headers;
        }
    }
}
