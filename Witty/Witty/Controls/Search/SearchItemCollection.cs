using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Witty.Controls.Search
{
    public class SearchItemCollection : IEnumerable<SearchItem>, INotifyCollectionChanged
    {
        public SearchItemCollection()
        {
            _items = new ObservableCollection<SearchItem>();
            _items.CollectionChanged += ItemsCollectionChanged;
        }

        public void AddSearchItem(SearchItem newItem)
        {
            if(newItem == null || 
               String.IsNullOrEmpty(newItem.Phrase) ||
                _items.Any(item => item.Phrase == newItem.Phrase))
            {
                return;
            }
            
            _items.Add(newItem);
        }

        public void Remove(SearchItem item)
        {
            _items.Remove(item);
        }

        public IEnumerator<SearchItem> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged = delegate { };

        void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged(this, e);
        }

        ObservableCollection<SearchItem> _items;        
    }
}
