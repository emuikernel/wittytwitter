using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Net;
using System.ComponentModel;

namespace Witty.Controls.Search
{
    public class TwitterSearchService : ITwitterSearchService
    {
        public void BeginSearch(SearchItem item)
        {                        
            var worker = new BackgroundWorker();
            worker.RunWorkerCompleted += (s,e) =>
            {
                try
                {
                    if (e.Result != null)
                    {
                        OnSearchComplete(e.Result as IEnumerable<SearchResult>);
                    }
                }
                catch(WebException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            };
            worker.DoWork += (s, e) =>
            {
                var client = new WebClient();
                var uri = new Uri(e.Argument.ToString());
                var xml = client.DownloadString(uri);
                if (!String.IsNullOrEmpty(xml))
                {
                    e.Result = ParseResults(xml, item);
                }
            };

            string url = String.Format(urlFormat, HttpUtility.UrlEncode(item.Phrase));
            worker.RunWorkerAsync(url);
        }        

        private IEnumerable<SearchResult> ParseResults(string xml, SearchItem item)
        {
            var doc = XDocument.Parse(xml);
            var converter = new TwitterSearchAtomConverter(doc, item);
            return converter.Convert();
        }

        private void OnSearchComplete(IEnumerable<SearchResult> results)
        {
            var handler = SearchComplete;
            if(handler != null && results != null && results.Any())
            {
                var args = new SearchCompleteEventArgs();
                args.Results = results.ToList();
                handler(this, args);
            }
        }

        public event EventHandler<SearchCompleteEventArgs> SearchComplete;

        private string urlFormat = "http://search.twitter.com/search.atom?q={0}&rpp=25";
    }
}
