using System;
using System.Collections.Generic;
using System.Text;

namespace TwitterLib
{
    class MyException : Exception
    {
        public MyException(string str)
        {
        }
    }

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
    }
}
