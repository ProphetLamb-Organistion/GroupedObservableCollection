using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace System.Collections.Specialized
{
    /// <inheritdoc cref="IObservableGroupCollection{TKey,TValue}" />
    [DebuggerDisplay("Groups = {GroupCount}, Items = {Count}")]
    public partial class ObservableGroupCollection<TKey, TValue>
        : ObservableCollection<TValue>, IObservableGroupCollection<TKey, TValue>
        where TKey : notnull
    {
        #region Fields

        protected List<SynchronizedObservableGrouping> m_groups = new List<SynchronizedObservableGrouping>();
        protected readonly IDictionary<TKey, SynchronizedObservableGrouping> m_syncedGroups;

        protected readonly IEqualityComparer<TKey> m_keyEqualityComparer;

        private readonly object __syncRoot = new object();

        private bool _throwOnBaseCall = true;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ObservableGroupCollection{TKey,TValue}"/>.
        /// </summary>
        public ObservableGroupCollection()
        {
            m_syncedGroups = new Dictionary<TKey, SynchronizedObservableGrouping>();
            m_keyEqualityComparer = EqualityComparer<TKey>.Default;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ObservableGroupCollection{TKey,TValue}"/> with the specified <see cref="IEqualityComparer{TKey}"/>.
        /// </summary>
        /// <param name="keyEqualityComparer">The equality comparer used to get the hashcode of keys, and to determine if two keys are equal.</param>
        public ObservableGroupCollection(IEqualityComparer<TKey>? keyEqualityComparer)
        {
            // Providing no EqualityComparer instead of the default reduces branching slightly in Dictionary.
            if (keyEqualityComparer is null)
            {
                m_syncedGroups = new Dictionary<TKey, SynchronizedObservableGrouping>();
                m_keyEqualityComparer = EqualityComparer<TKey>.Default;
            }
            else
            {
                m_syncedGroups = new Dictionary<TKey, SynchronizedObservableGrouping>(keyEqualityComparer);
                m_keyEqualityComparer = keyEqualityComparer;
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ObservableGroupCollection{TKey,TValue}"/> from existing <paramref name="groupings"/>.
        /// </summary>
        /// <param name="groupings">The data source of the collection.</param>
        public ObservableGroupCollection(IEnumerable<IGrouping<TKey, TValue>?> groupings) : this(groupings, null) { }

        /// <summary>
        /// Initializes a new instance of <see cref="ObservableGroupCollection{TKey,TValue}"/> from existing <paramref name="groupings"/>, with the specified <see cref="IEqualityComparer{TKey}"/>.
        /// </summary>
        /// <param name="groupings">The data source of the collection.</param>
        /// <param name="keyEqualityComparer">The equality comparer used to get the hashcode of keys, and to determine if two keys are equal.</param>
        public ObservableGroupCollection(IEnumerable<IGrouping<TKey, TValue>?> groupings,
            IEqualityComparer<TKey>? keyEqualityComparer) : this(keyEqualityComparer)
        {
            Debug.Assert(groupings != null);
            IList<TValue> items = Items;
            foreach (IGrouping<TKey, TValue>? g in groupings!)
            {
                if (g is null)
                    continue;
                if (g.Key is null)
                    throw new NullReferenceException("IGrouping.Key can not be null.");
                SynchronizedObservableGrouping group = new SynchronizedObservableGrouping(g.Key, this, __syncRoot);
                GroupAdd(group);
                foreach (TValue item in g)
                {
                    items.Add(item);
                    group.EndIndexExclusive++;
                }
                OffsetAfterGroup(group, group.Count);
            }
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public int GroupCount => m_groups.Count;

        /// <inheritdoc />
        IObservableGrouping<TKey, TValue> IObservableGroupCollection<TKey, TValue>.this[in TKey key] => this[key];
        
        /// <inheritdoc />
        IGrouping<TKey, TValue> IGroupCollection<TKey, TValue>.this[in TKey key] => this[key];

        /// <inheritdoc cref="IObservableGroupCollection{TKey,TValue}.this"/>
        public SynchronizedObservableGrouping this[in TKey key] =>
            GroupTryGet(key, out SynchronizedObservableGrouping group) ? group : throw new KeyNotFoundException();


        /// <summary>
        /// Indicates whether the collection is sorted. If true, can not Insert or Move
        /// </summary>
        public virtual bool IsSorted => false;

        #endregion

        #region Public members
        
        /// <inheritdoc />
        public void AddOrCreate(in TKey key, in TValue value)
        {
            BaseCallCheckin();

            if (!GroupTryGet(key, out SynchronizedObservableGrouping group))
            {
                group = new SynchronizedObservableGrouping(key, this, __syncRoot);
                GroupAdd(group);
            }
            GroupAddValue(group, value);

            BaseCallCheckout();
        }
        
        /// <inheritdoc />
        public void AddOrCreate(IGrouping<TKey, TValue> grouping) => AddOrCreate(grouping.Key, grouping);
        
        /// <inheritdoc />
        public void AddOrCreate(in TKey key, IEnumerable<TValue> values)
        {
            BaseCallCheckin();

            int itemsCount = 0;
            if (!GroupTryGet(key, out SynchronizedObservableGrouping group))
            {
                group = new SynchronizedObservableGrouping(key, this, __syncRoot);
                GroupAdd(group);
            }

            using IEnumerator<TValue> en = values.GetEnumerator();

            while (en.MoveNext())
            {
                GroupAddValue(group, en.Current, -1, false);
                itemsCount++;
            }

            group.EndIndexExclusive += itemsCount;
            OffsetAfterGroup(group, itemsCount);

            BaseCallCheckout();
        }

        /// <inheritdoc />
        IObservableGrouping<TKey, TValue> IGroupCollection<TKey, TValue>.Create(in TKey key) => Create(key);

        /// <inheritdoc cref="IGroupCollection{TKey,TValue}.Create" />
        public SynchronizedObservableGrouping Create(in TKey key)
        {
            BaseCallCheckin();

            if (m_syncedGroups.ContainsKey(key))
                throw new ArgumentOutOfRangeException(nameof(key));
            SynchronizedObservableGrouping created = new SynchronizedObservableGrouping(key, this, __syncRoot);
            GroupAdd(created);

            BaseCallCheckout();
            return created;
        }
        
        /// <inheritdoc />
        public bool Remove(in TKey key)
        {
            if (!GroupTryGet(key, out SynchronizedObservableGrouping group))
                return false;
            BaseCallCheckin();

            // Remove grouping definition
            OffsetAfterGroup(group, group.StartIndexInclusive - group.EndIndexExclusive);
            GroupRemove(group);
            // Remove items of removed grouping
            for (int i = group.EndIndexExclusive - 1; i >= group.StartIndexInclusive; i--)
            {
                RemoveAt(i);
            }

            BaseCallCheckout();
            return true;
        }
        
        /// <inheritdoc />
        bool IGroupCollection<TKey, TValue>.TryGetGrouping(in TKey key, out IGrouping<TKey, TValue> grouping)
        {
            bool result = GroupTryGet(key, out SynchronizedObservableGrouping g);
            grouping = g;
            return result;
        }
        
        /// <inheritdoc />
        public bool TryGetGrouping(in TKey key, out IObservableGrouping<TKey, TValue> grouping)
        {
            bool result = GroupTryGet(key, out SynchronizedObservableGrouping g);
            grouping = g;
            return result;
        }

        /// <inheritdoc cref="IGroupCollection{TKey,TValue}.TryGetGrouping" />
        public bool TryGetGrouping(in TKey key, out SynchronizedObservableGrouping observableGrouping) => GroupTryGet(key, out observableGrouping);
        
        /// <inheritdoc />
        public bool ContainsKey(in TKey key)
        {
            return !(key is null) && GroupTryGet(key, out _);
        }

        /// <summary>
        /// Returns groupings in the <see cref="IObservableGrouping{TKey, TValue}"/>, filtered based on a predicate.
        /// </summary>
        /// <param name="predicate">A function to test each grouping for a condition.</param>
        /// <returns>An <see cref="IEnumerable{IObservableGrouping{TKey, TValue}}"/> that contains groupings from the <see cref="IObservableGrouping{TKey, TValue}"/> that satisfy the condition.</returns>
        public IEnumerable<IObservableGrouping<TKey, TValue>> EnumerateGroupings(
            Func<IObservableGrouping<TKey, TValue>, bool>? predicate = null)
        {
            // Prevent caller code from casting to List<> and modifying m_groups.
            using List<SynchronizedObservableGrouping>.Enumerator en = m_groups.GetEnumerator();
            while (en.MoveNext())
            {
                SynchronizedObservableGrouping item = en.Current;
                if (predicate is null || predicate(item))
                    yield return item!;
            }
        }

        #endregion

        #region Private members

        /// <summary>
        /// Adds the specific <see cref="SynchronizedObservableGrouping"/> to the collection
        /// </summary>
        /// <param name="observableGrouping">The grouping to add.</param>
        protected virtual void GroupAdd(in SynchronizedObservableGrouping observableGrouping)
        {
            m_syncedGroups.Add(observableGrouping.Key, observableGrouping);
            m_groups.Add(observableGrouping);
        }

        /// <summary>
        /// Removes the specific <see cref="SynchronizedObservableGrouping"/> with all items from the collection.
        /// </summary>
        /// <param name="observableGrouping">The grouping to remove.</param>
        protected virtual void GroupRemove(in SynchronizedObservableGrouping observableGrouping)
        {
            m_syncedGroups.Remove(observableGrouping.Key);
            m_groups.Remove(observableGrouping);
        }

        /// <summary>
        /// Gets the grouping associated with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="syncedObservableGrouping">When this method returns, the grouping associated with the specified <paramref name="key"/>, if the <paramref name="key"/> is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns><see cref="true"/> if the <see cref="IObservableGrouping{TKey, TValue}"/> contains an grouping with the specified <paramref name="key"/>; otherwise, <see cref="false"/>.</returns>
        protected virtual bool GroupTryGet(in TKey key, out SynchronizedObservableGrouping syncedObservableGrouping)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            return m_syncedGroups.TryGetValue(key, out syncedObservableGrouping);
        }

        /// <summary>
        /// Adds a value to the specified group, preferably at the <paramref name="desiredIndex"/>.
        /// </summary>
        /// <param name="group">The group to which the item shall be added.</param>
        /// <param name="item">The item to add.</param>
        /// <param name="desiredIndex">The index at which the item should be added. -1 and group.Count add the item at the end of the group. Does not guarantee the eventual index of the item.</param>
        /// <param name="offset">Whether to offset the groups after, and increment this <paramref name="group"/>s EndIndexExclusive.</param>
        protected virtual void GroupAddValue(SynchronizedObservableGrouping group, TValue item, int desiredIndex = -1, bool offset = true)
        {
            Debug.Assert(desiredIndex == -1 || (uint)desiredIndex <= (uint)group.Count, "desiredIndex == -1 || (uint)desiredIndex <= (uint)group.Count");
            InsertItem( desiredIndex == -1 ? group.EndIndexExclusive : group.StartIndexInclusive + desiredIndex, item);
            if (offset)
            {
                OffsetAfterGroup(group, 1);
                group.EndIndexExclusive++;
            }
        }

        private void OffsetAfterGroup(in SynchronizedObservableGrouping emitter, int itemsCount)
        {
            IList<SynchronizedObservableGrouping> groups = m_groups;
            int index = groups.IndexOf(emitter);
            if (index == -1)
                throw new ArgumentException(nameof(emitter));
            for (int i = index + 1; i < groups.Count; i++)
            {
                groups[i].StartIndexInclusive += itemsCount;
                groups[i].EndIndexExclusive += itemsCount;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ThrowOnIllegalBaseCall([CallerMemberName] string callingFunction = null)
        {
            if (_throwOnBaseCall)
                throw new NotSupportedException(
                    "The operation \"" + callingFunction + "\" is not supported for GroupedObservableCollection. Use operations functions and properties exposed by IGroupedObservableCollection.");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void BaseCallCheckin() => _throwOnBaseCall = false;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void BaseCallCheckout() => _throwOnBaseCall = true;

        #endregion

        #region Overrides

        /// <summary>
        /// Called by base class ObservableCollection&lt;T&gt; when the list is being cleared
        /// </summary>
        protected override void ClearItems()
        {
            // Do not ThrowOnIllegalBaseCall
            base.ClearItems();

            // Clear groups
            m_syncedGroups.Clear();
            m_groups.Clear();
        }

        /// <summary>
        /// Called by base class ObservableCollection&lt;T&gt; when an item is removed from list.
        /// </summary>
        protected override void RemoveItem(int index)
        {
            ThrowOnIllegalBaseCall();
            base.RemoveItem(index);
        }

        /// <summary>
        /// Called by base class ObservableCollection&lt;T&gt; when an item is added to list.
        /// </summary>
        protected override void InsertItem(int index, TValue item)
        {
            ThrowOnIllegalBaseCall();
            base.InsertItem(index, item);
        }

        /// <summary>
        /// Called by base class ObservableCollection&lt;T&gt; when an item is set in list.
        /// </summary>
        protected override void SetItem(int index, TValue item)
        {
            ThrowOnIllegalBaseCall();
            base.SetItem(index, item);
        }
        
        /// <summary>
        /// Called by base class ObservableCollection&lt;T&gt; when an item is moved in list.
        /// </summary>
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            ThrowOnIllegalBaseCall();
            base.MoveItem(oldIndex, newIndex);
        }

        #endregion
    }
}