using System;
using System.Runtime.Serialization;

namespace TwitterLib
{
    /// <summary>
    /// Custom Exception when the per hour rate limit for authenticated Twitter API calls have been hit.
    /// </summary>
    [Serializable]
    public class RateLimitException : Exception
    {
        public RateLimitException()
        {
        }

        public RateLimitException(string message) : base(message)
        {
        }

        public RateLimitException(string message, Exception inner) : base(message, inner)
        {
        }

        protected RateLimitException(SerializationInfo info, StreamingContext context) : base(info, context) 
        { 
        }
    }

    [Serializable]
    public class ProxyAuthenticationRequiredException : Exception
    { 
        public ProxyAuthenticationRequiredException()
        {
        }

        public ProxyAuthenticationRequiredException(string message) : base(message)
        {
        }

        public ProxyAuthenticationRequiredException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ProxyAuthenticationRequiredException(SerializationInfo info, StreamingContext context) : base(info, context) 
        { 
        }
    }

    /// <summary>
    /// Custom exception for when the Twitter API is freaking out.
    /// </summary>
    [Serializable]
    public class BadGatewayException : Exception
    {
        public BadGatewayException() {}

        public BadGatewayException(string message) : base(message) { }

        public BadGatewayException(string message, Exception inner) : base(message, inner) { }

        protected BadGatewayException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
