using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Witty.Controls.Search
{
    public class SearchResultCollection : IEnumerable<SearchResult>, INotifyCollectionChanged
    {
        public SearchResultCollection(int maxSearchResults)
        {
            _maxSearchResults = maxSearchResults;
            _items = new ObservableCollection<SearchResult>();
            _items.CollectionChanged += ItemsCollectionChanged;                
        }

        public IEnumerator<SearchResult> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public void AddResults(IList<SearchResult> results)
        {
            foreach(var result in results)
            {
                if (IsValid(result) && !IsDuplicate(result))
                {
                    InsertItemInOrder(result);
                }

            }
            PruneOldResults();
        }

        public void RemoveResultsAssociatedwithItems(IEnumerable<SearchItem> deletedSearchItems)
        {
            foreach (var deleteItem in deletedSearchItems)
            {
                var removeableResults = _items.Where(item => item.AssociatedSearchItem == deleteItem).ToList();
                foreach(var result in removeableResults)
                {
                    _items.Remove(result);
                }
            }
        }

        private bool IsDuplicate(SearchResult result)
        {
            if(_items.Any(item => item == result))
            {
                return true;
            }
            return false;
        }

        private bool IsValid(SearchResult result)
        {
            return SearchResultValidationRules.All(r => r(result));
        }


        public event NotifyCollectionChangedEventHandler CollectionChanged;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<Predicate<SearchResult>> SearchResultValidationRules 
        { 
            get
            {
                yield return s => s != null;
                yield return s => s.User != null;
                yield return s => s.AssociatedSearchItem != null;                
                yield return s => !String.IsNullOrEmpty(s.User.Name);
                yield return s => !String.IsNullOrEmpty(s.Text);
            }        
        }

        private void InsertItemInOrder(SearchResult newItem)
        {
            var nextItem = _items.FirstOrDefault(item => item.DateCreated < newItem.DateCreated);
            
            if(nextItem == null)
            {
                _items.Add(newItem);
            }
            else
            {
                _items.Insert(_items.IndexOf(nextItem), newItem);
            }                                              
        }

        void PruneOldResults()
        {
            var numberToRemove = _items.Count - _maxSearchResults;
            if (numberToRemove > 0)
            {
                var itemsToRemove = _items.Take(numberToRemove).ToList();
                foreach (var item in itemsToRemove)
                {
                    _items.Remove(item);
                }
            }
        }

        void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var handler = CollectionChanged;
            if (handler != null) CollectionChanged(this, e);            
        }

        private int _maxSearchResults;
        ObservableCollection<SearchResult> _items;
    }
}
