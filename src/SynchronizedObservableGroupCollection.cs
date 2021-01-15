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
        [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
        public class SynchronizedObservableGroupCollection
              : ObservableCollection<ISynchronizedObservableGrouping<TKey, TValue>>,
                IObservableGroupCollection<TKey, TValue>,
                IReadOnlyDictionary<TKey, SynchronizedObservableGrouping>
        {
#region Fields

            protected Dictionary<TKey, SynchronizedObservableGrouping> m_keyDictionary;
            protected readonly ObservableGroupingCollection<TKey, TValue> m_valuesCollection;

#endregion

#region Constructors

            protected internal SynchronizedObservableGroupCollection(ObservableGroupingCollection<TKey, TValue> valuesCollection)
            {
                m_valuesCollection = valuesCollection;
                m_keyDictionary = new Dictionary<TKey, SynchronizedObservableGrouping>();
            }

#endregion

#region Properties
            
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

            /// <inheritdoc />
            public virtual bool IsSorted => false;

            /// <inheritdoc />
            public SynchronizedObservableGrouping this[TKey key] => m_keyDictionary[key];

            /// <inheritdoc />
            ISynchronizedObservableGrouping<TKey, TValue> IReadOnlyList<ISynchronizedObservableGrouping<TKey, TValue>>.this[int index] => this[index];

#endregion

#region Public members

            /// <inheritdoc />
            public bool ContainsKey(TKey key) => m_keyDictionary.ContainsKey(key);

            /// <inheritdoc />
            // Dictionary.Contains has superior performance compared to ObservableCollection.Contains.
            public new bool Contains(ISynchronizedObservableGrouping<TKey, TValue> item) => m_keyDictionary.ContainsKey(item.Key);

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

            void ICollection<ISynchronizedObservableGrouping<TKey, TValue>>.Add(ISynchronizedObservableGrouping<TKey, TValue> item)
            {
                if (!(item is SynchronizedObservableGrouping grouping))
                    throw new ArgumentException("The type of the item must derive from SynchronizedObservableGrouping.");
                Add(grouping);
            }
            
            /// <summary>Returns this instance of <see cref="ISynchronizedObservableGrouping{TKey,TValue}"/> cast to an Enumerable type.</summary>
            /// <remarks>Cannot use the grouping directly in a foreach loop or LINQ expression, because the class implements multiple IEnumerable interfaces, therefore the cast is ambiguous.</remarks>
            public IEnumerable<ISynchronizedObservableGrouping<TKey, TValue>> AsEnumerable => this;
            
            IEnumerator<ISynchronizedObservableGrouping<TKey, TValue>> IEnumerable<ISynchronizedObservableGrouping<TKey, TValue>>.GetEnumerator() => GetEnumerator();

            IEnumerator<KeyValuePair<TKey, SynchronizedObservableGrouping>> IEnumerable<KeyValuePair<TKey, SynchronizedObservableGrouping>>.GetEnumerator() => m_keyDictionary.GetEnumerator();

            internal void OffsetAfterGroup(in SynchronizedObservableGrouping emitter, int itemsCount)
            {
                int index = IndexOf(emitter);
                if (index == -1)
                    throw new ArgumentException(nameof(emitter));
                for (int i = index + 1; i < Items.Count; i++)
                {
                    GetItem(i).EndIndexExclusive += itemsCount;
                    GetItem(i).StartIndexInclusive += itemsCount;
                }
            }

#endregion

#region Private members

            private SynchronizedObservableGrouping GetItem(int index) => (SynchronizedObservableGrouping)Items[index];

#endregion

#region Overrides

            protected override void ClearItems()
            {
                m_keyDictionary.Clear();
                base.ClearItems();

                lock (m_valuesCollection)
                    m_valuesCollection.Clear();
            }

            protected override void InsertItem(int index, ISynchronizedObservableGrouping<TKey, TValue> item)
            {
                if (!(item is SynchronizedObservableGrouping grouping))
                    throw new ArgumentException("The type of the item must derive from SynchronizedObservableGrouping.");
                base.InsertItem(index, item);
                m_keyDictionary.Add(item.Key, grouping);

                int itemsIndex = index == 0 
                    ? 0
                    : Items[index - 1].EndIndexExclusive;
                grouping.EndIndexExclusive = itemsIndex;
                grouping.StartIndexInclusive = itemsIndex;
            }

            protected override void MoveItem(int oldIndex, int newIndex)
            {
                if (oldIndex == newIndex)
                    return;

                base.MoveItem(oldIndex, newIndex);

                SynchronizedObservableGrouping movedItem = GetItem(newIndex);
                int movedItemOldStartIndex = movedItem.StartIndexInclusive,
                    loIndex = Math.Min(newIndex, oldIndex),
                    hiIndex = Math.Max(newIndex, oldIndex);
                SynchronizedObservableGrouping previousItem;
                if (loIndex != 0)
                {
                    previousItem = GetItem(loIndex - 1);
                }
                else
                {
                    previousItem = GetItem(loIndex++);
                    int count = previousItem.Count;
                    previousItem.EndIndexExclusive = count;
                    previousItem.StartIndexInclusive = 0;
                }

                for (int i = loIndex; i <= hiIndex; i++)
                {
                    SynchronizedObservableGrouping item = GetItem(i);
                    int count = item.Count;
                    item.EndIndexExclusive = previousItem.EndIndexExclusive + count;
                    item.StartIndexInclusive = previousItem.EndIndexExclusive;
                    previousItem = item;
                }

                lock (m_valuesCollection)
                    m_valuesCollection.MoveRange(movedItemOldStartIndex, movedItem.StartIndexInclusive, movedItem.Count);
            }

            protected override void SetItem(int index, ISynchronizedObservableGrouping<TKey, TValue> item)
            {
                if (!(item is SynchronizedObservableGrouping newItem))
                    throw new ArgumentException("The type of the item must derive from SynchronizedObservableGrouping.");
                SynchronizedObservableGrouping oldItem = GetItem(index);
                int offset = item.Count - oldItem.Count;
                
                base.SetItem(index, item);
                m_keyDictionary.Remove(oldItem.Key);
                m_keyDictionary.Add(item.Key, newItem);

                newItem.EndIndexExclusive = index + item.Count;
                newItem.StartIndexInclusive = index;
                OffsetAfterGroup(newItem, offset);
            }

            protected override void RemoveItem(int index)
            {
                SynchronizedObservableGrouping item = GetItem(index);

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
