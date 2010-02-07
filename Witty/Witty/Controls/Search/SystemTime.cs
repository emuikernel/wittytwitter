using System;

namespace Witty.Controls.Search
{
    public static class SystemTime
    {
        public static Func<DateTime> Now 
        { 
            get
            {
                return _now;
            } 
            set
            {
                _now = value;
            }
        
        }

        private static Func<DateTime> _now = () => DateTime.Now;
    }
}
