using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Witty.Controls.Search;

namespace WittyUnitTests.Search
{
    public class FakeTwitterSearchService : ITwitterSearchService
    {
        public void BeginSearch(SearchItem item)
        {
            LastTermSeen = item.Phrase;
            RaiseCompleteEvent(item);
        }
       
        public void Reset()
        {
            LastTermSeen = null;
        }

        public string LastTermSeen { get; set; }

        public event EventHandler<SearchCompleteEventArgs> SearchComplete;

        private void RaiseCompleteEvent(SearchItem item)
        {
            var asm = Assembly.GetExecutingAssembly();
            var stream = asm.GetManifestResourceStream(_resourceName);

            if (stream != null && SearchComplete != null)
            {
                var doc = XDocument.Load(XmlReader.Create(stream));
                var converter = new TwitterSearchAtomConverter(doc, item);
                SearchComplete(this, new SearchCompleteEventArgs() { Results = converter.Convert().ToList()} );
            }
        }

        private string _resourceName = "WittyUnitTests.Search.SearchResult.xml";        
    }
}
