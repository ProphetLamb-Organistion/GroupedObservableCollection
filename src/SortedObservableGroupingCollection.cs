using GroupedObservableCollection.Import;

using System.Collections.Generic;
using System.Linq;

namespace System.Collections.Specialized
{
    /// <summary>
    /// Represents a sorted dynamic data collection of items grouped by keys, that provides notifications when items get added, removed, or when the whole list is refreshed.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of elements in the collection.</typeparam>
    /// <remarks>Insertion performance is sub optimal, because various operations must be performed on members of ObservableCollection, instead of directly on an Array.</remarks>
    public class SortedObservableGroupingCollection<TKey, TValue>
        : ObservableGroupingCollection<TKey, TValue>
        where TKey : notnull
    {
#region Fields

        private SynchronizedSortedObservableGroupCollection _groupings;

#endregion

#region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="SortedObservableGroupingCollection{TKey,TValue}"/>. 
        /// Requires <typeparamref name="TKey"/> and <typeparamref name="TValue"/> to implement <see cref="IComparable{TKey}"/> and <see cref="IComparable{TValue}"/> respectively.
        /// </summary>
        public SortedObservableGroupingCollection()
        {
            if (!typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey)))
                throw new ArgumentException("The generic type parameter TKey, does not implement IComparable<TKey>.");
            if (!typeof(IComparable<TValue>).IsAssignableFrom(typeof(TValue)))
                throw new ArgumentException("The generic type parameter TValue, does not implement IComparable<TValue>.");
            Comparer = Comparer<TValue>.Default;
            m_groupings = _groupings = new SynchronizedSortedObservableGroupCollection(m_groupings, this, Comparer<TKey>.Default);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SortedObservableGroupingCollection{TKey,TValue}"/> from existing groupings. 
        /// Requires <typeparamref name="TKey"/> and <typeparamref name="TValue"/> to implement <see cref="IComparable{TKey}"/> and <see cref="IComparable{TValue}"/> respectively.
        /// </summary>
        public SortedObservableGroupingCollection(IEnumerable<IGrouping<TKey, TValue>?> groupings)
            : this()
        {
            CopyFrom(groupings ?? throw new ArgumentNullException(nameof(groupings)));
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SortedObservableGroupingCollection{TKey,TValue}"/> with the specified key, and value comparer.
        /// </summary>
        /// <param name="keyComparer">The comparer used to sort keys.</param>
        /// <param name="valueComparer">The comparer used to sort values.</param>
        public SortedObservableGroupingCollection(IComparer<TKey> keyComparer, IComparer<TValue> valueComparer)
        {
            m_groupings = _groupings = new SynchronizedSortedObservableGroupCollection(m_groupings, this, keyComparer);
            Comparer = valueComparer;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SortedObservableGroupingCollection{TKey,TValue}"/> from existing groupings, with the specified key and value comparer.
        /// </summary>
        /// <param name="groupings">The data source of the collection.</param>
        /// <param name="keyComparer">The comparer used to sort keys.</param>
        /// <param name="valueComparer">The comparer used to sort values.</param>
        public SortedObservableGroupingCollection(
            IEnumerable<IGrouping<TKey, TValue>?> groupings,
            IComparer<TKey> keyComparer,
            IComparer<TValue> valueComparer)
            : this(keyComparer, valueComparer)
        {
            CopyFrom(groupings ?? throw new ArgumentNullException(nameof(groupings)));
        }
        
#endregion

#region Properties

        /// <summary>
        /// Returns the instance of the comparer used to compare values.
        /// The default comparer, if none was provided in the constructor.
        /// </summary>
        public IComparer<TValue> Comparer { get; }

        /// <inheritdoc />
        public override bool IsSorted => true;

#endregion

#region Overrides

        /// <inheritdoc />
        protected override void GroupAdd(SynchronizedObservableGrouping grouping, TValue item, int relativeIndex = -1)
        {
            int index = CollectionSortHelper<TValue>.BinarySearch(this, 0, grouping.Count, item, Comparer);
            if (index < 0)
                index = ~index; // No result: insert between greater and smaller
            else
                index++; // Found equal item: insert after existing
            base.GroupAdd(grouping, item, index);
        }

        /// <inheritdoc />
        protected override void GroupAdd(SynchronizedObservableGrouping grouping, IReadOnlyList<TValue> items, int relativeIndex = -1)
        {
            foreach (var item in items)
            {
                GroupAdd(grouping, item);
            }
        }

        protected override void SetItem(int index, TValue item)
        {
            ThrowOnIllegalBaseCall();
            base.SetItem(index, item);
        }

#endregion
    }
}