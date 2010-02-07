using System;
using TwitterLib;

namespace Witty.Controls.Search
{
    public class SearchResult : Tweet
    {
        public SearchItem AssociatedSearchItem { get; set; }

        public bool Equals(SearchResult other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return CompareTweets(other);
        }

        private bool CompareTweets(SearchResult other)
        {
            bool result = false;
            if (User != null && other.User != null)
            {
                if (Text == other.Text &&
                   User.Name == other.User.Name)
                {
                    result = true;
                }
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(SearchResult)) return false;
            return Equals((SearchResult)obj);
        }

        public static bool operator ==(SearchResult left, SearchResult right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SearchResult left, SearchResult right)
        {
            return !Equals(left, right);
        }
    }
}
