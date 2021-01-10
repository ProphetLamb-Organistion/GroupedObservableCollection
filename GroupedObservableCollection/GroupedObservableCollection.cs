using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace System.Collections.Specialized
{
    [DebuggerDisplay("Groups = {GroupCount}, Items = {Count}")]
    public partial class ObservableGroupCollection<TKey, TValue>
        : ObservableCollection<TValue>, IObservableGroupCollection<TKey, TValue>
    {
        #region Fields

        protected List<SynchronizedGrouping> m_groups = new List<SynchronizedGrouping>();
        protected readonly IDictionary<TKey, SynchronizedGrouping> m_syncedGroups;

        protected readonly IEqualityComparer<TKey> m_keyEqualityComparer;

        private SynchronizedGrouping? _previousHit;

        private readonly object __syncRoot = new object();

        private bool _throwOnBaseCall = true;

        #endregion

        #region Constructors

        public ObservableGroupCollection(IEqualityComparer<TKey>? keyEqualityComparer = null)
        {
            m_keyEqualityComparer = keyEqualityComparer ?? EqualityComparer<TKey>.Default;
            m_syncedGroups = new Dictionary<TKey, SynchronizedGrouping>(m_keyEqualityComparer);
        }

        public ObservableGroupCollection(IEnumerable<IGrouping<TKey, TValue>?> groupings,
            IEqualityComparer<TKey>? keyEqualityComparer = null) : this(keyEqualityComparer)
        {
            Debug.Assert(groupings != null);
            IList<TValue> items = Items;
            foreach (IGrouping<TKey, TValue>? g in groupings!)
            {
                if (g is null)
                    continue;
                if (g.Key is null)
                    throw new NullReferenceException("IGrouping.Key can not be null.");
                SynchronizedGrouping grouping = new SynchronizedGrouping(g.Key, this, __syncRoot);
                InternalAddGroup(grouping);
                foreach (TValue item in g)
                {
                    items.Add(item);
                    grouping.EndIndexExclusive++;
                }
            }
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public int GroupCount => m_groups.Count;
        
        /// <inheritdoc />
        public IObservableGrouping<TKey, TValue> this[in TKey key] => InternalTryGetGrouping(key, out SynchronizedGrouping group) ? group : throw new KeyNotFoundException();
        
        /// <inheritdoc />
        IGrouping<TKey, TValue> IGroupCollection<TKey, TValue>.this[in TKey key] => this[key];

        #endregion

        #region Public members
        
        /// <inheritdoc />
        public void AddOrCreate(in TKey key, in TValue value)
        {
            BaseCallCheckin();

            if (!InternalTryGetGrouping(key, out SynchronizedGrouping group))
            {
                group = new SynchronizedGrouping(key, this, __syncRoot);
                InternalAddGroup(group);
                Add(value);
            }
            else
            {
                Insert(group.EndIndexExclusive, value);
            }
            group.EndIndexExclusive++;

            OffsetGroupingsAfter(group, 1);

            BaseCallCheckout();
        }
        
        /// <inheritdoc />
        public void AddOrCreate(IGrouping<TKey, TValue> grouping) => AddOrCreate(grouping.Key, grouping);
        
        /// <inheritdoc />
        public void AddOrCreate(in TKey key, IEnumerable<TValue> values)
        {
            BaseCallCheckin();

            using IEnumerator<TValue> en = values.GetEnumerator();

            int itemsCount = 0;
            if (!InternalTryGetGrouping(key, out SynchronizedGrouping group))
            {
                group = new SynchronizedGrouping(key, this, __syncRoot);
                InternalAddGroup(group);
                while (en.MoveNext())
                {
                    Add(en.Current);
                    itemsCount++;
                }
            }
            else
            {
                while (en.MoveNext())
                {
                    Insert(group.EndIndexExclusive + itemsCount, en.Current);
                    itemsCount++;
                }
            }

            group.EndIndexExclusive += itemsCount;
            OffsetGroupingsAfter(group, itemsCount);

            BaseCallCheckout();
        }
        
        /// <inheritdoc />
        public IObservableGrouping<TKey, TValue> Create(in TKey key)
        {
            BaseCallCheckin();

            if (m_syncedGroups.ContainsKey(key))
                throw new ArgumentOutOfRangeException(nameof(key));
            SynchronizedGrouping created = new SynchronizedGrouping(key, this, __syncRoot);
            InternalAddGroup(created);

            BaseCallCheckout();

            return created;
        }
        
        /// <inheritdoc />
        public bool Remove(in TKey key)
        {
            if (!InternalTryGetGrouping(key, out SynchronizedGrouping group))
                return false;
            BaseCallCheckin();

            // Remove grouping definition
            OffsetGroupingsAfter(group, group.StartIndexInclusive - group.EndIndexExclusive);
            InternalRemoveGroup(key, group);
            // Remove items of removed grouping
            for (int i = group.EndIndexExclusive - 1; i >= group.StartIndexInclusive; i--)
            {
                RemoveAt(i);
            }
            // Reset previous grouping returned by InternalTryGetGrouping, to ensure that no dead grouping evaluates true
            _previousHit = null;

            BaseCallCheckout();
            return true;
        }
        
        /// <inheritdoc />
        public bool TryGetGrouping(in TKey key, out IObservableGrouping<TKey, TValue> grouping)
        {
            bool result = InternalTryGetGrouping(key, out SynchronizedGrouping g);
            grouping = g;
            return result;
        }
        
        /// <inheritdoc />
        public bool TryGetGrouping(in TKey key, out IGrouping<TKey, TValue> grouping)
        {
            bool result = InternalTryGetGrouping(key, out SynchronizedGrouping g);
            grouping = g;
            return result;
        }
        
        /// <inheritdoc />
        public bool ContainsKey(in TKey key)
        {
            return !(key is null) && InternalTryGetGrouping(key, out _);
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
            using List<SynchronizedGrouping>.Enumerator en = m_groups.GetEnumerator();
            while (en.MoveNext())
            {
                SynchronizedGrouping item = en.Current;
                if (predicate is null || predicate(item))
                    yield return item!;
            }
        }

        #endregion

        #region Private members

        protected virtual void InternalAddGroup(in SynchronizedGrouping grouping)
        {
            m_syncedGroups.Add(grouping.Key, grouping);
            m_groups.Add(grouping);
        }

        protected virtual void InternalRemoveGroup(in TKey key, in SynchronizedGrouping grouping)
        {
            m_syncedGroups.Remove(key);
            m_groups.Remove(grouping);
        }

        protected virtual bool InternalTryGetGrouping(in TKey key, out SynchronizedGrouping syncedGrouping)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));
            if (!(_previousHit is null) && m_keyEqualityComparer.Equals(key, _previousHit.Key))
            {
                syncedGrouping = _previousHit;
                return true;
            }

            if (m_syncedGroups.TryGetValue(key, out syncedGrouping))
            {
                _previousHit = syncedGrouping;
                return true;
            }

            _previousHit = default;
            return false;
        }

        protected virtual void OffsetGroupingsAfter(in SynchronizedGrouping emitter, int itemsCount)
        {
            IList<SynchronizedGrouping> groups = m_groups;
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
        private void ThrowOnIllegalBaseCall([CallerMemberName] string callingFunction = null)
        {
            if (_throwOnBaseCall)
                throw new NotSupportedException(
                    "The operation \"" + callingFunction + "\" is not supported for GroupedObservableCollection. Use operations functions and properties exposed by IGroupedObservableCollection.");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BaseCallCheckin() => _throwOnBaseCall = false;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BaseCallCheckout() => _throwOnBaseCall = true;
        
        #endregion

        #region Overrides

        /// <summary>
        /// Called by base class ObservableCollection&lt;T&gt; when the list is being cleared
        /// </summary>
        protected override void ClearItems()
        {
            // Do not ThrowOnIllegalBaseCall();
            base.ClearItems();
            // Clear groups
            m_syncedGroups.Clear();
            m_groups.Clear();
            // Reset InternalTryGetGrouping
            _previousHit = null;
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