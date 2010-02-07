namespace Witty.Controls.Search
{
    public class SearchConfiguration
    {        
        public int MinSecondsBetweenItemSearch { get; set; }
        public int MaxSearchResults { get; set; }
        public ITwitterSearchService SearchService { get; set; }
        public ISearchTimer Timer { get; set; }

        public static SearchConfiguration Default = new SearchConfiguration
        {
            MinSecondsBetweenItemSearch = 300,
            MaxSearchResults = 100,
            SearchService = new TwitterSearchService(),
            Timer = new SearchTimer()
        };
    }
}
