using System;

namespace LTC2.Shared.Http.Exceptions
{
    public class InvalidValueException : Exception
    {
        public InvalidValueException(string message) : base(message)
        {

        }
    }
}
