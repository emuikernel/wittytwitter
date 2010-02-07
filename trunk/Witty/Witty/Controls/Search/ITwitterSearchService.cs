using System;
using System.Xml.Linq;

namespace Witty.Controls.Search
{
    public interface ITwitterSearchService
    {
        void BeginSearch(SearchItem term);
        event EventHandler<SearchCompleteEventArgs> SearchComplete;
    }
}
