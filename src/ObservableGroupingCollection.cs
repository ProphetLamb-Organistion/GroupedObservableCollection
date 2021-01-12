﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace System.Collections.Specialized
{
    /// <inheritdoc cref="IObservableGroupingCollection{TKey,TValue}" />
    [DebuggerDisplay("Groups = {m_groupings.Count}, Items = {Count}")]
    public partial class ObservableGroupingCollection<TKey, TValue>
        : ObservableCollection<TValue>, IObservableGroupingCollection<TKey, TValue>
        where TKey : notnull
    {
        #region Fields

        protected SynchronizedObservableGroupCollection m_groupings;

        private bool _throwOnBaseCall = true;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ObservableGroupingCollection{TKey,TValue}"/>.
        /// </summary>
        public ObservableGroupingCollection()
        {
            m_groupings = new SynchronizedObservableGroupCollection(this, null);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ObservableGroupingCollection{TKey,TValue}"/> with the specified <see cref="IEqualityComparer{TKey}"/>.
        /// </summary>
        /// <param name="keyEqualityComparer">The equality comparer used to get the hashcode of keys, and to determine if two keys are equal.</param>
        public ObservableGroupingCollection(IEqualityComparer<TKey>? keyEqualityComparer)
        {
            m_groupings = new SynchronizedObservableGroupCollection(this, keyEqualityComparer);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ObservableGroupingCollection{TKey,TValue}"/> from existing <paramref name="groupings"/>.
        /// </summary>
        /// <param name="groupings">The data source of the collection.</param>
        public ObservableGroupingCollection(IEnumerable<IGrouping<TKey, TValue>?> groupings)
            : this(groupings, null) { }

        /// <summary>
        /// Initializes a new instance of <see cref="ObservableGroupingCollection{TKey,TValue}"/> from existing <paramref name="groupings"/>, with the specified <see cref="IEqualityComparer{TKey}"/>.
        /// </summary>
        /// <param name="groupings">The data source of the collection.</param>
        /// <param name="keyEqualityComparer">The equality comparer used to get the hashcode of keys, and to determine if two keys are equal.</param>
        public ObservableGroupingCollection(IEnumerable<IGrouping<TKey, TValue>?> groupings, IEqualityComparer<TKey>? keyEqualityComparer)
            : this(keyEqualityComparer)
        {
            CopyFrom(groupings ?? throw new ArgumentNullException(nameof(groupings)));
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        IObservableGrouping<TKey, TValue> IObservableGroupingCollection<TKey, TValue>.this[in TKey key] => this[key];

        /// <inheritdoc />
        IGrouping<TKey, TValue> IGroupingCollection<TKey, TValue>.this[in TKey key] => this[key];

        /// <inheritdoc cref="IObservableGroupingCollection{TKey,TValue}.this"/>
        public SynchronizedObservableGrouping this[in TKey key] => m_groupings[key];


        /// <summary>
        /// Indicates whether the collection is sorted. If true, can not Insert or Move
        /// </summary>
        public virtual bool IsSorted => false;

        /// <inheritdoc />
        IObservableGroupCollection<TKey, TValue> IObservableGroupingCollection<TKey, TValue>.Groupings => Groupings;

        /// <inheritdoc cref="IObservableGroupingCollection{TKey,TValue}.Groupings" />
        public SynchronizedObservableGroupCollection Groupings => m_groupings;

        IReadOnlyList<IGrouping<TKey, TValue>> IGroupingCollection<TKey, TValue>.Groups => m_groupings;

        #endregion

        #region Public members

        /// <inheritdoc />
        public void Add(TKey key, TValue value)
        {
            var grouping = m_groupings.GetOrAdd(key, () => new SynchronizedObservableGrouping(key, this));

            GroupAdd(grouping, value);
        }
        
        /// <inheritdoc />
        public void Add(IGrouping<TKey, TValue> grouping) => Add(grouping.Key, grouping);
        
        /// <inheritdoc />
        public void Add(TKey key, IEnumerable<TValue> values)
        {
            SynchronizedObservableGrouping grouping = m_groupings.GetOrAdd(key, () => new SynchronizedObservableGrouping(key, this));

            foreach (TValue item in values)
            {
                GroupAdd(grouping, item);
            }
        }

        /// <inheritdoc />
        IGrouping<TKey, TValue> IGroupingCollection<TKey, TValue>.Create(in TKey key) => Create(key);

        /// <inheritdoc cref="IGroupingCollection{TKey,TValue}.Create" />
        public SynchronizedObservableGrouping Create(in TKey key)
        {
            if (m_groupings.ContainsKey(key))
                throw new ArgumentOutOfRangeException(nameof(key));
            
            SynchronizedObservableGrouping created = new SynchronizedObservableGrouping(key, this);
            m_groupings.Add(created);

            return created;
        }
        
        /// <inheritdoc />
        public bool Remove(in TKey key)
        {
            if (!m_groupings.TryGetValue(key, out SynchronizedObservableGrouping group))
                return false;
            m_groupings.Remove(group);
            return true;
        }

        #endregion

        #region Private members

        private void CopyFrom(IEnumerable<IGrouping<TKey, TValue>?> groupings)
        {
            foreach (IGrouping<TKey, TValue>? g in groupings)
            {
                if (g is null)
                    continue;
                if (g.Key is null)
                    throw new NullReferenceException("IGrouping.Key can not be null.");
                SynchronizedObservableGrouping grouping = new SynchronizedObservableGrouping(g.Key, this)
                { 
                    EndIndexExclusive = Count,
                    StartIndexInclusive = Count
                };
                m_groupings.Add(grouping);
                foreach (TValue item in g)
                {
                    Items.Add(item);
                    grouping.EndIndexExclusive++;
                    m_groupings.OffsetAfterGroup(grouping, 1);
                }
            }
        }

        /// <summary>
        /// Adds a value to the specified group, preferably at the <paramref name="desiredIndex"/>.
        /// </summary>
        /// <param name="group">The group to which the item shall be added.</param>
        /// <param name="item">The item to add.</param>
        /// <param name="desiredIndex">The index at which the item should be added. -1 and group.Count add the item at the end of the group. Does not guarantee the eventual index of the item.</param>
        protected virtual void GroupAdd(SynchronizedObservableGrouping group, TValue item, int desiredIndex = -1)
        {
            Debug.Assert(desiredIndex == -1 || (uint)desiredIndex <= (uint)group.Count, "desiredIndex == -1 || (uint)desiredIndex <= (uint)group.Count");

            group.EndIndexExclusive++;
            m_groupings.OffsetAfterGroup(group, 1);
            
            BaseCallCheckin();
            InsertItem( desiredIndex == -1 ? group.EndIndexExclusive - 1 /* EndIndexExclusive incremented before call */ : group.StartIndexInclusive + desiredIndex, item);
            BaseCallCheckout();
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ThrowOnIllegalBaseCall([CallerMemberName] string? callingFunction = null)
        {
            if (_throwOnBaseCall)
                throw new NotSupportedException(
                    "The operation \"" + callingFunction + "\" is not supported in GroupedObservableCollection.");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void BaseCallCheckin() => _throwOnBaseCall = false;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void BaseCallCheckout() => _throwOnBaseCall = true;

        /// <summary>
        /// Removes a section from the collection.
        /// </summary>
        /// <param name="startIndex">The index of the first element to remove.</param>
        /// <param name="count">The number of element to remove</param>
        protected virtual void RemoveRange(int startIndex, int count)
        {
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (Count < startIndex + count)
                throw new IndexOutOfRangeException();
            TValue[] items = new TValue[count];
            int index = startIndex + count - 1;
            for (int i = count - 1; i >= 0; i--, index--)
            {
                items[i] = Items[index];
                Items.RemoveAt(index);
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items, startIndex));
        }

        /// <summary>
        /// Inserts elements at the specified index.
        /// </summary>
        /// <param name="startIndex">The index at which to insert the first element.</param>
        /// <param name="items">The collection of element to insert.</param>
        /// <param name="itemsIndex">The index of the first element in the <paramref name="items"/> collection to insert.</param>
        /// <param name="itemsCount">The number of elements to insert.</param>
        protected virtual void InsertRange(int startIndex, IReadOnlyList<TValue> items, int itemsIndex, int itemsCount)
        {
            if ((uint)startIndex > (uint)Count)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (itemsIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(itemsIndex));
            if (itemsCount < 0)
                throw new ArgumentOutOfRangeException(nameof(itemsCount));
            if (items.Count < itemsIndex + itemsCount)
                throw new IndexOutOfRangeException();
            int index = startIndex;
            for (int i = itemsIndex; i < itemsCount; i++, index++)
            {
                Items.Insert(index, items[i]);
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items, startIndex));
        }
        
        /// <summary>
        /// Moves a section of elements in the collection.
        /// </summary>
        /// <param name="oldIndex">The starting index of elements to move.</param>
        /// <param name="newIndex">The starting index to which to move the items.</param>
        /// <param name="count">The number of elements to move.</param>
        protected virtual void MoveRange(int oldIndex, int newIndex, int count)
        {
            TValue[] items = new TValue[count];
            var offsetStartIndex = oldIndex;
            var offsetTargetIndex = newIndex;
            for (var i = count - 1; i >= 0; i--) {
                TValue item = Items[offsetStartIndex + i];
                items[i] = item;

                Items.RemoveAt(offsetStartIndex + i);
                if (offsetTargetIndex > offsetStartIndex + i)
                    offsetTargetIndex -= 1;

                Items.Insert(offsetTargetIndex, item);            
                if (offsetStartIndex > offsetTargetIndex)
                    offsetStartIndex += 1;            
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, items, newIndex, oldIndex));
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Called by base class ObservableCollection&lt;T&gt; when the list is being cleared
        /// </summary>
        protected override void ClearItems()
        {
            base.ClearItems();
            m_groupings.Clear();
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