using System.Runtime.Serialization;


namespace RolyPoly.Core.Utilities
{
    public class RequestThrottleException : Exception
    {
        public RequestThrottleException()
        {
        }

        public RequestThrottleException(string? message) : base(message)
        {
        }

        public RequestThrottleException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
