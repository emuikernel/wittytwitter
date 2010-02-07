using System;

namespace Witty.Controls.Search
{
    public class SearchItem
    {
        public static SearchItem CreateFrom(string word)
        {
            return new SearchItem
            {
                Phrase = word,
                LastSearch = DateTime.MinValue
            };
        }

        public string Phrase { get; set; }
        public DateTime LastSearch { get; set; }
    }
}
