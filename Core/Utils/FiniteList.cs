using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace BLELocator.Core.Utils
{
    public sealed class FiniteList<T> : ObservableCollection<T>
    {
        private readonly int _maxSize;
        public FiniteList(int maxSize)
        {
            if (maxSize < 2)
                throw new Exception(string.Format("maxSize [{0}] needs to be bigger than 1", maxSize));
            _maxSize = maxSize;
            CollectionChanged += EnforceMaxSize;
        }

        private void EnforceMaxSize(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Count > _maxSize)
            {
                RemoveAt(0);
            }
        }

        
        
    }
}