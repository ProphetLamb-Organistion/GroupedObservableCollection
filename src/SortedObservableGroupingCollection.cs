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

        private SynchronizedSortedObservableGroupCollection _groups;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="SortedObservableGroupingCollection{TKey,TValue}"/> with the specified key, and value comparer.
        /// </summary>
        /// <param name="keyComparer">The comparer used to sort keys.</param>
        /// <param name="valueComparer">The comparer used to sort values.</param>
        public SortedObservableGroupingCollection(IComparer<TKey> keyComparer, IComparer<TValue> valueComparer)
        {
            m_groupings = _groups = new SynchronizedSortedObservableGroupCollection(m_groupings, this, null, keyComparer);
            Comparer = valueComparer;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SortedObservableGroupingCollection{TKey,TValue}"/> with the specified equality, key and value comparer.
        /// </summary>
        /// <param name="keyEqualityComparer">The equality comparer used to get the hashcode of keys, and to determine if two keys are equal.</param>
        /// <param name="keyComparer">The comparer used to sort keys.</param>
        /// <param name="valueComparer">The comparer used to sort values.</param>
        public SortedObservableGroupingCollection(
            IEqualityComparer<TKey> keyEqualityComparer,
            IComparer<TKey> keyComparer,
            IComparer<TValue> valueComparer)
            : base(keyEqualityComparer)
        {
            m_groupings = _groups = new SynchronizedSortedObservableGroupCollection(m_groupings, this, keyEqualityComparer, keyComparer);
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
            : base(groupings)
        {
            m_groupings = _groups = new SynchronizedSortedObservableGroupCollection(m_groupings, this, null, keyComparer);
            Comparer = valueComparer;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SortedObservableGroupingCollection{TKey,TValue}"/> from existing groupings, with the specified equality, key and value comparer.
        /// </summary>
        /// <param name="groupings">The data source of the collection.</param>
        /// <param name="keyEqualityComparer">The equality comparer used to get the hashcode of keys, and to determine if two keys are equal.</param>
        /// <param name="keyComparer">The comparer used to sort keys.</param>
        /// <param name="valueComparer">The comparer used to sort values.</param>
        public SortedObservableGroupingCollection(
            IEnumerable<IGrouping<TKey, TValue>?> groupings,
            IEqualityComparer<TKey> keyEqualityComparer,
            IComparer<TKey> keyComparer,
            IComparer<TValue> valueComparer)
            : base(groupings, keyEqualityComparer)
        {
            m_groupings = _groups = new SynchronizedSortedObservableGroupCollection(m_groupings, this, keyEqualityComparer, keyComparer);
            Comparer = valueComparer;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the instance of the comparer used to compare values.
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