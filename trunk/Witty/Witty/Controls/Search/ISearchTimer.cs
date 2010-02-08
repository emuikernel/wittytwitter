using System;

namespace Witty.Controls.Search
{
    public interface ISearchTimer
    {
        event EventHandler<EventArgs> Tick;
    }
}
