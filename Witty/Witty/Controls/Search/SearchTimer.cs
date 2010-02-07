using System;
using System.Windows.Threading;

namespace Witty.Controls.Search
{
    class SearchTimer : ISearchTimer
    {
        public SearchTimer()
        {
            _nativetimer = new DispatcherTimer();

            _nativetimer.Interval = TimeSpan.FromSeconds(10);
            _nativetimer.Tick += (s, e) =>
            {
                var handler = Tick;
                if (handler != null) handler(this, e);
            };

            _nativetimer.Start();  
        }

        public event EventHandler<EventArgs> Tick;

        private readonly DispatcherTimer _nativetimer;        
    }
}
