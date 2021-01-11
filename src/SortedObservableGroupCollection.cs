using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using GroupedObservableCollection.Utility;

namespace System.Collections.Specialized
{
    /// <summary>
    /// Represents a sorted dynamic data collection of items grouped by keys, that provides notifications when items get added, removed, or when the whole list is refreshed.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of elements in the collection.</typeparam>
    /// <remarks>Insertion performance is sub optimal, because various operations must be performed on members of ObservableCollection, instead of directly on an Array.</remarks>
    public class SortedObservableGroupCollection<TKey, TValue>
        : ObservableGroupCollection<TKey, TValue>
        where TKey : notnull
    {
        #region Fields

        private readonly IComparer<SynchronizedObservableGrouping>? _wrappedKeyComparer;

        #endregion

        #region Constructors

        /// <inheritdoc />
        public SortedObservableGroupCollection()
            : base()
        { }
        
        /// <inheritdoc />
        public SortedObservableGroupCollection(IEqualityComparer<TKey> keyEqualityComparer)
            : base(keyEqualityComparer)
        { }
        
        /// <inheritdoc />
        public SortedObservableGroupCollection(IEnumerable<IGrouping<TKey, TValue>> groupings)
            : base(groupings)
        { }
        
        /// <inheritdoc />
        public SortedObservableGroupCollection(IEnumerable<IGrouping<TKey, TValue>> groupings, IEqualityComparer<TKey> keyEqualityComparer)
            : base(groupings, keyEqualityComparer)
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="SortedObservableGroupCollection{TKey,TValue}"/> with the specified key, and value comparer.
        /// </summary>
        /// <param name="keyComparer">The comparer used to sort keys.</param>
        /// <param name="valueComparer">The comparers used to sort values.</param>
        public SortedObservableGroupCollection(IComparer<TKey>? keyComparer, IComparer<TValue>? valueComparer)
            : base()
        {
            KeyComparer = keyComparer;
            ValueComparer = valueComparer;
            _wrappedKeyComparer = keyComparer is null
                ? null
                : Comparer<SynchronizedObservableGrouping>.Create((x, y) => KeyComparer!.Compare(x.Key, y.Key));
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SortedObservableGroupCollection{TKey,TValue}"/> with the specified equality, key and value comparer.
        /// </summary>
        /// <param name="keyEqualityComparer">The equality comparer used to get the hashcode of keys, and to determine if two keys are equal.</param>
        /// <param name="keyComparer">The comparer used to sort keys.</param>
        /// <param name="valueComparer">The comparer used to sort values.</param>
        public SortedObservableGroupCollection(
            IEqualityComparer<TKey> keyEqualityComparer,
            IComparer<TKey>? keyComparer,
            IComparer<TValue>? valueComparer)
            : base(keyEqualityComparer)
        {
            KeyComparer = keyComparer;
            ValueComparer = valueComparer;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SortedObservableGroupCollection{TKey,TValue}"/> from existing groupings, with the specified key and value comparer.
        /// </summary>
        /// <param name="groupings">The data source of the collection.</param>
        /// <param name="keyComparer">The comparer used to sort keys.</param>
        /// <param name="valueComparer">The comparer used to sort values.</param>
        public SortedObservableGroupCollection(
            IEnumerable<IGrouping<TKey, TValue>?> groupings,
            IComparer<TKey>? keyComparer,
            IComparer<TValue>? valueComparer)
            : base(groupings)
        {
            KeyComparer = keyComparer;
            ValueComparer = valueComparer;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SortedObservableGroupCollection{TKey,TValue}"/> from existing groupings, with the specified equality, key and value comparer.
        /// </summary>
        /// <param name="groupings">The data source of the collection.</param>
        /// <param name="keyEqualityComparer">The equality comparer used to get the hashcode of keys, and to determine if two keys are equal.</param>
        /// <param name="keyComparer">The comparer used to sort keys.</param>
        /// <param name="valueComparer">The comparer used to sort values.</param>
        public SortedObservableGroupCollection(
            IEnumerable<IGrouping<TKey, TValue>?> groupings,
            IEqualityComparer<TKey> keyEqualityComparer,
            IComparer<TKey>? keyComparer,
            IComparer<TValue>? valueComparer)
            : base(groupings, keyEqualityComparer)
        {
            KeyComparer = keyComparer;
            ValueComparer = valueComparer;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the instance of comparer used to sort keys.
        /// </summary>
        public IComparer<TKey>? KeyComparer { get; }
        /// <summary>
        /// Returns the instance of the comparer used to sort values.
        /// </summary>
        public IComparer<TValue>? ValueComparer { get; }

        /// <inheritdoc />
        public override bool IsSorted => ValueComparer != null;

        #endregion

        #region Overrides

        /// <inheritdoc />
        protected override void GroupAddValue(SynchronizedObservableGrouping group, TValue item, int desiredIndex = -1, bool offset = true)
        {
            if (!(ValueComparer is null))
            {
                desiredIndex = CollectionSortHelper<TValue>.BinarySearch(
                    this,
                    group.StartIndexInclusive,
                    group.Count, 
                    item,
                    ValueComparer);
            }
            base.GroupAddValue(group, item, desiredIndex, offset);
        }

        /// <inheritdoc />
        protected override void GroupAdd(in SynchronizedObservableGrouping observableGrouping)
        {
            // Add group
            base.GroupAdd(in observableGrouping);

            if (KeyComparer is null)
                return;
            // Find sorted position
            int index = m_groups.Count - 1;
            int swapIndex = CollectionSortHelper<SynchronizedObservableGrouping>.BinarySearch(
                m_groups,
                0,
                index,
                observableGrouping,
                _wrappedKeyComparer);
            
            BaseCallCheckin();

            this.MoveItem(index, swapIndex);

            BaseCallCheckout();
        }

        #endregion
    }
}
