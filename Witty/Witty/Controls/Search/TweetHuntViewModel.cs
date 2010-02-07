using System;
using System.ComponentModel;


namespace Witty.Controls.Search
{
    public class TweetHuntViewModel : INotifyPropertyChanged
    {
        public TweetHuntViewModel()
            : this(SearchConfiguration.Default)

        {
        }

        public TweetHuntViewModel(SearchConfiguration configuration)
        {
            _searchItems = new SearchItemCollection();
            _searchResults  = new SearchResultCollection(configuration.MaxSearchResults);
            _coordinator = new SearchCoordinator(this, configuration);
        }

        public void AddCurrentWordAsSearchTerm()
        {            
            _searchItems.AddSearchItem(SearchItem.CreateFrom(CurrentWord));
            CurrentWord = String.Empty;
        }

        public void DeleteTerm(SearchItem item)
        {
            _searchItems.Remove(item);
        }
        
        public string CurrentWord
        {
            get { return _currentWord; }
            set
            {
                if (_currentWord != value)
                {
                    _currentWord = value;
                    RaisePropertyChangedEvent("CurrentWord");
                }
            }
        }

        public SearchItemCollection SearchItems
        {
            get
            {
                return _searchItems;   
            }
        }

        public SearchResultCollection SearchResults
        {
            get 
            { 
                return _searchResults;             
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        

        private void RaisePropertyChangedEvent(string propertyName)
        {
            var args = new PropertyChangedEventArgs(propertyName);
            PropertyChanged(this, args);
        }

        private SearchItemCollection _searchItems;
        private SearchResultCollection _searchResults;
        private string _currentWord;
        private SearchCoordinator _coordinator;
    }
}
