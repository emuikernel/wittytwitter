using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace TwitterLib
{
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
