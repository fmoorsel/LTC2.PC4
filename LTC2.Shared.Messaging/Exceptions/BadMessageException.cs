using System;

namespace LTC2.Shared.Messaging.Exceptions
{
    public class BadMessageException : Exception
    {
        public BadMessageException(string message) : base(message)
        {
        }
    }
}
