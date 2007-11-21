using System;
using System.Runtime.Serialization;

namespace TwitterLib
{
    /// <summary>
    /// Custom Exception when the 70/hour rate limit for authenticated Twitter API calls have been hit.
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
}
