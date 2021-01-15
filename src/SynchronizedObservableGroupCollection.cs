using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using GroupedObservableCollection.Import;

namespace System.Collections.Specialized
{
    public partial class ObservableGroupingCollection<TKey, TValue>
    {
        /// <summary>
        /// Represents a class managing the groupings in a <see cref="ObservableGroupingCollection{TKey,TValue}"/>.
        /// </summary>
        [DebuggerDisplay("Count = {Count}")]
        public class SynchronizedObservableGroupCollection
              : ObservableCollection<SynchronizedObservableGrouping>,
                IObservableGroupCollection<TKey, TValue>,
                IReadOnlyDictionary<TKey, SynchronizedObservableGrouping>
        {
#region Fields

            protected Dictionary<TKey, SynchronizedObservableGrouping> m_keyDictionary;
            protected readonly ObservableGroupingCollection<TKey, TValue> m_valuesCollection;

#endregion

#region Constructors

            protected internal SynchronizedObservableGroupCollection(ObservableGroupingCollection<TKey, TValue> valuesCollection, IEqualityComparer<TKey>? keyEqualityComparer)
            {
                m_valuesCollection = valuesCollection;
                m_keyDictionary = keyEqualityComparer is null
                    ? new Dictionary<TKey, SynchronizedObservableGrouping>()
                    : new Dictionary<TKey, SynchronizedObservableGrouping>(keyEqualityComparer);
                KeyEqualityComparer = keyEqualityComparer;
            }

#endregion

#region Properties

            public IEqualityComparer<TKey>? KeyEqualityComparer { get; }

            public IEnumerable<TKey> Keys => m_keyDictionary.Keys;

            public IEnumerable<SynchronizedObservableGrouping> Values => m_keyDictionary.Values;

            bool ICollection.IsSynchronized => true;

            object ICollection.SyncRoot
            {
                get
                {
                    lock (m_valuesCollection)
                        return m_valuesCollection;
                }
            }

            /// <inheritdoc />
            public bool IsReadOnly => false;

            /// <summary>
            /// Indicates whether the collection is sorted. If true, can not Insert or Move
            /// </summary>
            public virtual bool IsSorted => false;

            /// <inheritdoc />
            public SynchronizedObservableGrouping this[TKey key] => m_keyDictionary[key];

            /// <inheritdoc />
            IObservableGrouping<TKey, TValue> IReadOnlyList<IObservableGrouping<TKey, TValue>>.this[int index] => this[index];

#endregion

#region Public members

            /// <inheritdoc />
            public bool ContainsKey(TKey key) => m_keyDictionary.ContainsKey(key);

            public bool Contains(IObservableGrouping<TKey, TValue> item) => m_keyDictionary.ContainsKey(item.Key);

            /// <inheritdoc />
            public bool TryGetValue(TKey key, out SynchronizedObservableGrouping value) => m_keyDictionary.TryGetValue(key, out value);

            /// <summary>
            /// Adds a key/value pair to the <see cref="SynchronizedObservableGroupCollection"/> if the <paramref name="key"/> does not already exist. Returns the new value, or the existing value if the key already exists.
            /// </summary>
            /// <param name="key">The key of the element to add.</param>
            /// <param name="factory">The function used to generate a value for the key.</param>
            /// <returns>The value for the <paramref name="key"/>. This will be either the existing value for the <paramref name="key"/> if the key is already in the dictionary, or the new value if the <paramref name="key"/> was not in the dictionary.</returns>
            public SynchronizedObservableGrouping GetOrAdd(TKey key, Func<SynchronizedObservableGrouping> factory)
            {
                if (TryGetValue(key, out SynchronizedObservableGrouping grouping))
                    return grouping;
                Add(grouping = factory());
                return grouping;
            }

            void ICollection<IObservableGrouping<TKey, TValue>>.Add(IObservableGrouping<TKey, TValue> item)
            {
                if (!(item is SynchronizedObservableGrouping grouping))
                    throw new ArgumentException("The type of the item must derive from SynchronizedObservableGrouping.");
                Add(grouping);
            }

            public virtual bool Remove(IObservableGrouping<TKey, TValue> item)
            {
                var comparer = KeyEqualityComparer ?? EqualityComparer<TKey>.Default;
                for (int i = 0; i < Count; i++)
                {
                    if (comparer.Equals(Items[i].Key, item.Key))
                    {
                        RemoveItem(i);
                        return true;
                    }
                }

                return false;
            }

            public void CopyTo(IObservableGrouping<TKey, TValue>[] array, int arrayIndex)
            {
                if (arrayIndex < 0)
                    throw new ArgumentOutOfRangeException(nameof(arrayIndex));
                if (Count > arrayIndex + array.Length)
                    throw new IndexOutOfRangeException();
                int index = arrayIndex;
                for (int i = 0; i < Count; i++)
                    array[index++] = this[i];
            }

            /// <summary>Returns this instance of <see cref="SynchronizedObservableGroupCollection"/> cast to an Enumerable type.</summary>
            /// <remarks>Cannot use the <see cref="SynchronizedObservableGroupCollection"/> directly (in a e.g. foreach loop or LINQ expression), because the class implements multiple IEnumerable interfaces, therefore the cast is ambiguous.</remarks>
            public IEnumerable<SynchronizedObservableGrouping> AsEnumerable => this;
            
            IEnumerator<IObservableGrouping<TKey, TValue>> IEnumerable<IObservableGrouping<TKey, TValue>>.GetEnumerator() => GetEnumerator();

            IEnumerator<KeyValuePair<TKey, SynchronizedObservableGrouping>> IEnumerable<KeyValuePair<TKey, SynchronizedObservableGrouping>>.GetEnumerator() => m_keyDictionary.GetEnumerator();

            internal void OffsetAfterGroup(in SynchronizedObservableGrouping emitter, int itemsCount)
            {
                int index = IndexOf(emitter);
                if (index == -1)
                    throw new ArgumentException(nameof(emitter));
                if (itemsCount < 0)
                {
                    for (int i = index + 1; i < Items.Count; i++)
                    {
                        Items[i].StartIndexInclusive += itemsCount;
                        Items[i].EndIndexExclusive += itemsCount;
                    }
                }
                else
                {
                    for (int i = index + 1; i < Items.Count; i++)
                    {
                        Items[i].EndIndexExclusive += itemsCount;
                        Items[i].StartIndexInclusive += itemsCount;
                    }
                }
            }

#endregion

#region Overrides

            protected override void ClearItems()
            {
                m_keyDictionary.Clear();
                base.ClearItems();

                lock (m_valuesCollection)
                    m_valuesCollection.Clear();
            }

            protected override void InsertItem(int index, SynchronizedObservableGrouping item)
            {
                base.InsertItem(index, item);
                m_keyDictionary.Add(item.Key, item);

                int itemsIndex = index == 0 
                    ? 0
                    : Items[index - 1].EndIndexExclusive;
                item.EndIndexExclusive = itemsIndex;
                item.StartIndexInclusive = itemsIndex;
            }

            protected override void MoveItem(int oldIndex, int newIndex)
            {
                if (oldIndex == newIndex)
                    return;

                base.MoveItem(oldIndex, newIndex);

                SynchronizedObservableGrouping movedItem = Items[newIndex];
                int movedItemOldStartIndex = movedItem.StartIndexInclusive,
                    loIndex = Math.Min(newIndex, oldIndex),
                    hiIndex = Math.Max(newIndex, oldIndex);
                SynchronizedObservableGrouping? previousItem = loIndex < 1 ? null : Items[loIndex - 1];
                if (previousItem is null)
                {
                    previousItem = Items[loIndex++];
                    int count = previousItem.Count;
                    previousItem.EndIndexExclusive = count;
                    previousItem.StartIndexInclusive = 0;
                }
                for (int i = loIndex; i <= hiIndex; i++)
                {
                    SynchronizedObservableGrouping item = Items[i];
                    int count = item.Count;
                    item.EndIndexExclusive = previousItem.EndIndexExclusive + count;
                    item.StartIndexInclusive = previousItem.EndIndexExclusive;
                    previousItem = item;
                }

                lock (m_valuesCollection)
                    m_valuesCollection.MoveRange(movedItemOldStartIndex, movedItem.StartIndexInclusive, movedItem.Count);
            }

            protected override void SetItem(int index, SynchronizedObservableGrouping item)
            {
                SynchronizedObservableGrouping oldItem = this[index];
                int offset = item.Count - oldItem.Count;
                
                base.SetItem(index, item);
                m_keyDictionary.Remove(oldItem.Key);
                m_keyDictionary.Add(item.Key, item);

                item.EndIndexExclusive = index + item.Count;
                item.StartIndexInclusive = index;
                OffsetAfterGroup(item, offset);
            }

            protected override void RemoveItem(int index)
            {
                SynchronizedObservableGrouping item = this[index];

                OffsetAfterGroup(item, -item.Count);

                m_keyDictionary.Remove(item.Key);
                base.RemoveItem(index);

                lock (m_valuesCollection)
                    m_valuesCollection.RemoveRange(item.StartIndexInclusive, item.Count);
            }

#endregion
        }
    }
}
