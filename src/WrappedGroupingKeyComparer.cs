using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace GroupedObservableCollection
{
    internal class WrappedGroupingKeyComparer<TKey, TValue>
        : IComparer<ISynchronizedObservableGrouping<TKey, TValue>>
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

        public int Compare(ISynchronizedObservableGrouping<TKey, TValue> x, ISynchronizedObservableGrouping<TKey, TValue> y)
        {
            return KeyComparer.Compare(x.Key, y.Key);
        }
    }
}
