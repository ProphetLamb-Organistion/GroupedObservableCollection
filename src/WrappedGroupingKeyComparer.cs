using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace GroupedObservableCollection
{
    internal class WrappedGroupingKeyComparer<TKey, TValue>
        : IComparer<IObservableGrouping<TKey, TValue>>
        where TKey : notnull
    {

        public WrappedGroupingKeyComparer(IComparer<TKey> keyComparer)
        {
            KeyComparer = keyComparer;
        }

        /// <summary>
        /// Represents the instance of <see cref="IComparer{TKey}"/> used for comparisons.
        /// </summary>
        public IComparer<TKey> KeyComparer { get; }

        public int Compare(IObservableGrouping<TKey, TValue> x, IObservableGrouping<TKey, TValue> y)
        {
            return KeyComparer.Compare(x.Key, y.Key);
        }
    }
}
