using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TwitterLib;

namespace Witty.Controls.Search
{
    public class TwitterSearchAtomConverter
    {
        public TwitterSearchAtomConverter(XDocument atomDoc, SearchItem item)
        {
            _document = atomDoc;
            _item = item;
        }

        public IEnumerable<SearchResult> Convert()
        {
            var feed = _document.Element(ns + "feed");
            if(feed != null)
            {
                return feed.Elements(ns + "entry")             
                           .Select(e => ToSearchResult(e));
            }
            return Enumerable.Empty<SearchResult>();
        }     
   
        private SearchResult ToSearchResult(XElement entry)
        {
            var text = (string) entry.Element(ns + "title");
            var dateCreated = ParseDateTime((string) entry.Element(ns + "updated"));
            var source = (string) entry.Element(twitter + "source");

            var user = new User();            
            ExtractImage(entry, user);
            ExtractAuthor(entry, user);

            var result = new SearchResult
            {
                AssociatedSearchItem = _item,
                Text = text,
                DateCreated = dateCreated,
                Source = source,
                User = user
            };

            result.UpdateRelativeTime();
            return result;
        }

        private void ExtractAuthor(XElement entry, User user)
        {
            var author = entry.Element(ns + "author");
            if (author != null)
            {
                user.Name = (string)author.Element(ns + "name");
                var uri = (string)author.Element(ns + "uri");
                if (uri != null)
                {
                    var index = uri.LastIndexOf("/");
                    if (index + 1 < uri.Length)
                    {
                        user.ScreenName = uri.Substring(index + 1);
                    }
                }
            }
        }

        private void ExtractImage(XElement entry, User user)
        {
            var link = entry.Elements(ns + "link")
                .Where(e => (string) e.Attribute("rel") == "image")
                .FirstOrDefault();
            if (link != null)
            {
                user.ImageUrl = (string)link.Attribute("href");
            }
        }


        private DateTime? ParseDateTime(string value)
        {
            DateTime? result = null;

            DateTime temp;
            if(DateTime.TryParse(value, out temp))
            {
                result = temp;   
            }

            return result;
        }

        private XDocument _document;        
        private SearchItem _item;
        private static readonly XNamespace ns = "http://www.w3.org/2005/Atom";
        private static readonly XNamespace twitter = "http://api.twitter.com/";
    }   
}
