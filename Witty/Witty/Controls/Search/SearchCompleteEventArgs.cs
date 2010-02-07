using System;
using System.Collections.Generic;

namespace Witty.Controls.Search
{
    public class SearchCompleteEventArgs : EventArgs
    {
        public IList<SearchResult> Results
        {
            get; set;
        }
    }
}
