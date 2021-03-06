﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;


/*
 * The Dotnet runtime (currently) does not support processing multiple changes in a CollectionView.
 * Thus, for each operation on a range of items, only the first item is going to be processed, e.g. InsertRange(0, new[] { [1],[2],[3],[4] }) is analogous to Insert(0, [1]) from the perspective of the CollectionView.
 * The only way to circumvent this behaviour is to invoke the CollectionChanged event per item changed, instead of on a list of items. This introduces additional overhead.
 * Depending on, if the behaviour changes: Compile with COLLECTIONVIEW_EVENT_MULTICHANGE_SUPPORT defined or not.
 *
 * See: https://source.dot.net/PresentationFramework/System/Windows/Data/CollectionView.cs.html#https://source.dot.net/PresentationFramework/System/Windows/Data/CollectionView.cs.html,4270b8e1bdd07308
 */

namespace System.Collections.Specialized
{
    /// <inheritdoc cref="IObservableGroupingCollection{TKey,TValue}" />
    [DebuggerDisplay("Groups = {" + GroupingsCountPropertyName + "}, Items = {" + CountPropertyName + "}")]
    [Serializable]
    public partial class ObservableGroupingCollection<TKey, TValue>
        : ObservableCollection<TValue>, IObservableGroupingCollection<TKey, TValue>
        where TKey : notnull
    {
#region Fields

        internal const string GroupingsCountPropertyName = "m_groupings.Count";
        internal const string CountPropertyName = "Count";
        internal const string IndexerPropertyName = "Item[]";

        protected SynchronizedObservableGroupCollection m_groupings;

        [field: NonSerialized]
        private bool _throwOnBaseCall = true;

#endregion

#region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ObservableGroupingCollection{TKey,TValue}"/>.
        /// </summary>
        public ObservableGroupingCollection()
        {
            m_groupings = new SynchronizedObservableGroupCollection(this);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ObservableGroupingCollection{TKey,TValue}"/> from existing <paramref name="groupings"/>.
        /// </summary>
        /// <param name="groupings">The data source of the collection.</param>
        public ObservableGroupingCollection(IEnumerable<IGrouping<TKey, TValue>?> groupings)
            : this()
        {
            CopyFrom(groupings ?? throw new ArgumentNullException(nameof(groupings)));
        }

#endregion

#region Properties

        ISynchronizedObservableGrouping<TKey, TValue> IObservableGroupingCollection<TKey, TValue>.this[in TKey key] => this[key];

        IGrouping<TKey, TValue> IGroupingCollection<TKey, TValue>.this[in TKey key] => this[key];

        /// <summary>
        /// Gets the grouping with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the grouping to get.</param>
        /// <returns>The grouping with the specified <paramref name="key"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="key"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when there is no grouping with the <paramref name="key"/> in the collection.</exception>
        [IndexerName("Item")] // Equal to ObservableCollection.this[] IndexerName attribute.
        public SynchronizedObservableGrouping this[in TKey key] => m_groupings[key];


        /// <inheritdoc />
        public virtual bool IsSorted => false;

        IObservableGroupCollection<TKey, TValue> IObservableGroupingCollection<TKey, TValue>.Groupings => Groupings;

        /// <inheritdoc cref="IObservableGroupingCollection{TKey,TValue}.Groupings" />
        public SynchronizedObservableGroupCollection Groupings => m_groupings;

        IReadOnlyList<IGrouping<TKey, TValue>> IGroupingCollection<TKey, TValue>.Groupings => m_groupings;

#endregion

#region Public members

        /// <inheritdoc />
        public void Add(TKey key, TValue value)
        {
            SynchronizedObservableGrouping grouping = m_groupings.GetOrAdd(key, () => GroupingFactory(key));
            GroupAdd(grouping, value);
        }

        /// <inheritdoc />
        public void Add(IGrouping<TKey, TValue> grouping) => Add(grouping.Key, grouping);

        /// <inheritdoc />
        public void Add(TKey key, IEnumerable<TValue> values)
        {
            SynchronizedObservableGrouping grouping = m_groupings.GetOrAdd(key, () => GroupingFactory(key));
            GroupAdd(grouping, values.ToList());
        }

        /// <inheritdoc />
        IGrouping<TKey, TValue> IGroupingCollection<TKey, TValue>.Create(in TKey key) => Create(key);

        /// <inheritdoc cref="IGroupingCollection{TKey,TValue}.Create" />
        public SynchronizedObservableGrouping Create(in TKey key)
        {
            if (m_groupings.ContainsKey(key))
                throw new ArgumentOutOfRangeException(nameof(key));

            SynchronizedObservableGrouping created = GroupingFactory(key);
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

        protected void CopyFrom(IEnumerable<IGrouping<TKey, TValue>?> groupings)
        {
            CheckReentrancy();
            using (BlockReentrancy())
            {
                foreach (IGrouping<TKey, TValue>? g in groupings)
                {
                    if (g is null)
                        continue;
                    if (g.Key is null)
                        throw new NullReferenceException("IGrouping.Key can not be null.");
                    if (!m_groupings.TryGetValue(g.Key, out SynchronizedObservableGrouping grouping))
                    {
                        grouping = GroupingFactory(g.Key);
                        m_groupings.Add(grouping);
                    }
                    int index = grouping.EndIndexExclusive;
                    foreach (TValue item in g)
                    {
                        Items.Insert(index++, item);
                    }
                    grouping.EndIndexExclusive = index;
                }
            }
        }

        /// <summary>
        /// Adds a value to the specified grouping, preferably at the <paramref name="relativeIndex"/>.
        /// </summary>
        /// <param name="grouping">The grouping to which the item shall be added.</param>
        /// <param name="item">The item to add.</param>
        /// <param name="relativeIndex">T>The index relative to the StartIndex of the grouping, at which the item should be added. Does not guarantee the eventual index of the item.</param>
        protected virtual void GroupAdd(SynchronizedObservableGrouping grouping, TValue item, int relativeIndex = -1)
        {
            CheckReentrancy();
            if (relativeIndex != -1 && (uint)relativeIndex > (uint)grouping.Count)
                throw new ArgumentOutOfRangeException(nameof(relativeIndex));

            grouping.EndIndexExclusive++;
            m_groupings.OffsetAfterGroup(grouping, 1);

            BaseCallCheckin();
            InsertItem(relativeIndex == -1 ? grouping.EndIndexExclusive - 1 /* EndIndexExclusive incremented before call */ : grouping.StartIndexInclusive + relativeIndex, item);
            BaseCallCheckout();

            OnPropertyChanged(CountPropertyName);
            OnPropertyChanged(IndexerPropertyName);
        }

        /// <summary>
        /// Adds a list of items to the specified grouping at a index relative to the StartIndex of the grouping.
        /// </summary>
        /// <param name="grouping">The grouping to which the items shall be added.</param>
        /// <param name="items">The list of items to add. Is passed as parameter to the CollectionChanged event!</param>
        /// <param name="relativeIndex">The index relative to the StartIndex of the grouping, at which to insert the first item. Does not guarantee the eventual index of the items.</param>
        protected virtual void GroupAdd(SynchronizedObservableGrouping grouping, IReadOnlyList<TValue> items, int relativeIndex = -1)
        {
#if COLLECTIONVIEW_EVENT_MULTICHANGE_SUPPORT
            CheckReentrancy();
            if (relativeIndex < -1 || relativeIndex > grouping.EndIndexExclusive)
                throw new ArgumentOutOfRangeException(nameof(relativeIndex));
            int insertionIndex = relativeIndex == -1 ? grouping.EndIndexExclusive : grouping.StartIndexInclusive + relativeIndex;

            foreach (var item in items)
            {
                Items.Insert(insertionIndex++, item);
            }

            grouping.EndIndexExclusive += items.Count;
            m_groupings.OffsetAfterGroup(grouping, items.Count);

            OnCollectionChanged(NotifyCollectionChangedAction.Add, items, insertionIndex - Items.Count);
            OnPropertyChanged(CountPropertyName);
            OnPropertyChanged(IndexerPropertyName);
#else
            BaseCallCheckin();
            if (relativeIndex == -1 || relativeIndex == grouping.Count)
            {
                foreach (TValue item in items)
                    GroupAdd(grouping, item);
            }
            else
            {
                int index = relativeIndex;
                foreach (TValue item in items)
                    GroupAdd(grouping, item, index++);
            }
            BaseCallCheckout();
#endif
        }


        /// <summary>
        /// Removes a section from the collection. Only invoking NotifyCollectionChanged event upon completion.
        /// </summary>
        /// <param name="startIndex">The index of the first element to remove.</param>
        /// <param name="count">The number of element to remove</param>
        protected virtual void RemoveRange(int startIndex, int count)
        {
            CheckReentrancy();
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (Count < startIndex + count)
                throw new ArgumentOutOfRangeException(nameof(count));

#if COLLECTIONVIEW_EVENT_MULTICHANGE_SUPPORT
            TValue[] notifyBuffer = new TValue[count];
            InternalRemoveRange(startIndex, count, notifyBuffer);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, notifyBuffer, startIndex);
            OnPropertyChanged(CountPropertyName);
            OnPropertyChanged(IndexerPropertyName);
#else
            BaseCallCheckin();
            for (int i = startIndex + count - 1; i >= startIndex; i--)
                RemoveItem(i);
            BaseCallCheckout();
#endif
        }

        /// <summary>
        /// Inserts elements at the specified index. Only invoking NotifyCollectionChanged event upon completion.
        /// </summary>
        /// <param name="startIndex">The index at which to insert the first element.</param>
        /// <param name="items">The collection of element to insert.</param>
        protected virtual void InsertRange(int startIndex, IReadOnlyList<TValue> items)
        {
            CheckReentrancy();
            if ((uint)startIndex > (uint)Count)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

#if COLLECTIONVIEW_EVENT_MULTICHANGE_SUPPORT
            InternalInsertRange(startIndex, items);

            OnCollectionChanged(NotifyCollectionChangedAction.Add, items, startIndex);
            OnPropertyChanged(CountPropertyName);
            OnPropertyChanged(IndexerPropertyName);
#else
            BaseCallCheckin();
            int index = startIndex;
            for (int i = 0; i < items.Count; i++, index++)
                InsertItem(index, items[i]);
            BaseCallCheckout();
#endif
        }

        /// <summary>
        /// Moves a section of elements in the collection. Only invoking NotifyCollectionChanged event upon completion.
        /// </summary>
        /// <param name="oldIndex">The starting index of elements to move.</param>
        /// <param name="newIndex">The starting index to which to move the items.</param>
        /// <param name="count">The number of elements to move.</param>
        protected virtual void MoveRange(int oldIndex, int newIndex, int count)
        {
            CheckReentrancy();
            if ((uint)oldIndex >= (uint)Count)
                throw new ArgumentOutOfRangeException(nameof(oldIndex));
            if ((uint)newIndex >= (uint)Count)
                throw new ArgumentNullException(nameof(newIndex));
            if (Math.Max(oldIndex, newIndex) + count > Count)
                throw new ArgumentOutOfRangeException(nameof(count));
            
            TValue[] movedItems = new TValue[count];
#if COLLECTIONVIEW_EVENT_MULTICHANGE_SUPPORT
            InternalRemoveRange(oldIndex, count, movedItems);
            InternalInsertRange(newIndex, movedItems);

            OnCollectionChanged(NotifyCollectionChangedAction.Move, movedItems, newIndex, oldIndex);
            OnPropertyChanged(IndexerPropertyName);
#else
            BaseCallCheckin();
            int index = oldIndex + count - 1;
            for (int i = count - 1; i >= 0; i--, index--)
            {
                movedItems[i] = Items[index];
                RemoveItem(index);
            }

            index = newIndex;
            for (int i = 0; i < count; i++, index++)
            {
                InsertItem(index, movedItems[i]);
            }
            BaseCallCheckout();
#endif
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SynchronizedObservableGrouping"/>, with the specified key-value.
        /// </summary>
        /// <param name="key">The key associated with the instance.</param>
        /// <returns>A new instance of <see cref="SynchronizedObservableGrouping"/>.</returns>
        protected virtual SynchronizedObservableGrouping GroupingFactory(TKey key)
        {
            return new SynchronizedObservableGrouping(key, this);
        }

        protected void ThrowOnIllegalBaseCall([CallerMemberName] string? callingFunction = null)
        {
            if (_throwOnBaseCall)
                throw new NotSupportedException("The operation \"" + callingFunction + "\" is not supported in GroupedObservableCollection.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void BaseCallCheckin() => _throwOnBaseCall = false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void BaseCallCheckout() => _throwOnBaseCall = true;

        
#if COLLECTIONVIEW_EVENT_MULTICHANGE_SUPPORT
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InternalRemoveRange(int startIndex, int count, TValue[] notifyBuffer)
        {
            int index = startIndex + count - 1;
            for (int i = count - 1; i >= 0; i--, index--)
            {
                notifyBuffer.SetValue(Items[index], i);
                Items.RemoveAt(index);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InternalInsertRange(int startIndex, IReadOnlyList<TValue> items)
        {
            int index = startIndex;
            for (int i = 0; i < items.Count; i++, index++)
            {
                Items.Insert(index, items[i]);
            }
        }
#endif

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>Use where (action & Add | Remove) != 0</summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, IReadOnlyList<TValue> changedItems, int startIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, changedItems, startIndex));
        }
        
        /// <summary>Use where action == Move</summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, IReadOnlyList<TValue> changedItems, int newIndex, int oldIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, changedItems, newIndex, oldIndex));
        }

#endregion

#region Overrides
        /*
         * Base method invoke PropertyChanged events for Count and Item[] when appropriate. There's no need for a custom implementation
         */

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