using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Witty.Controls.Search
{
    class SearchCoordinator
    {
        public SearchCoordinator(TweetHuntViewModel viewModel,
                                 SearchConfiguration configuration)
        {
            _model = viewModel;
            _timer = configuration.Timer;
            _searchService = configuration.SearchService;
            
            _model.SearchItems.CollectionChanged += SearchItemsCollectionChanged;
            _timer.Tick += (s, e) => InitiateSearch();
            _searchService.SearchComplete += SearchComplete;

            _minSecondsBetweenItemSearch = configuration.MinSecondsBetweenItemSearch;            
        }

        public void InitiateSearch()
        {
            var searchItem =
                (
                    from item in _model.SearchItems
                    let time = (SystemTime.Now() - item.LastSearch).TotalSeconds
                    where time > _minSecondsBetweenItemSearch
                    orderby time descending
                    select item
                ).FirstOrDefault();

            if (searchItem != null)
            {
                searchItem.LastSearch = SystemTime.Now();
                _searchService.BeginSearch(searchItem);
            }
        }

        private void SearchComplete(object sender, SearchCompleteEventArgs e)
        {

            _model.SearchResults.AddResults(e.Results);
        }        


        private void SearchItemsCollectionChanged(object sender, 
                                                  NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Add)
            {
                InitiateSearch();
            }
            if(e.Action == NotifyCollectionChangedAction.Remove)
            {
                var newItems = Enumerable.Empty<SearchItem>();
                var oldItems = Enumerable.Empty<SearchItem>();
                
                if(e.NewItems != null)
                {
                    newItems = e.NewItems.OfType<SearchItem>();
                }
                if(e.OldItems != null)
                {
                    oldItems = e.OldItems.OfType<SearchItem>();
                }
                var difference = oldItems.Where(item => !newItems.Contains(item));

                _model.SearchResults.RemoveResultsAssociatedwithItems(difference);
            }
        }

        private TweetHuntViewModel _model;
        private int _minSecondsBetweenItemSearch;
        private ITwitterSearchService _searchService;
        private ISearchTimer _timer;
    }
}
