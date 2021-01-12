using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace System.Collections.Specialized
{
    public partial class ObservableGroupingCollection<TKey, TValue>
    {
        /// <summary>
        /// Represents a class managing the groupings in a <see cref="ObservableGroupingCollection{TKey,TValue}"/>.
        /// </summary>
        [DebuggerDisplay("Count = {Count}")]
        public class SynchronizedObservableGroupCollection
            : ObservableCollection<SynchronizedObservableGrouping>, IReadonlyObservableCollection<IObservableGrouping<TKey, TValue>>, IReadOnlyDictionary<TKey, SynchronizedObservableGrouping>, ICollection
        {
            #region Fields

            protected Dictionary<TKey, SynchronizedObservableGrouping> m_keyDictionary;
            protected readonly ObservableGroupingCollection<TKey, TValue> m_valuesCollection;

            #endregion

            #region Constructors

            protected internal SynchronizedObservableGroupCollection(ObservableGroupingCollection<TKey, TValue> valuesCollection, IEqualityComparer<TKey>? equalityComparer)
            {
                m_valuesCollection = valuesCollection;
                m_keyDictionary = equalityComparer is null 
                    ? new Dictionary<TKey, SynchronizedObservableGrouping>()
                    : new Dictionary<TKey, SynchronizedObservableGrouping>(equalityComparer);
                EqualityComparer = equalityComparer;
            }

            #endregion

            #region Properties

            /// <summary>
            /// Indicates whether the collection is sorted. If true, can not Insert or Move
            /// </summary>
            public virtual bool IsSorted => false;

            public IEqualityComparer<TKey>? EqualityComparer { get; }

            IEnumerable<TKey> IReadOnlyDictionary<TKey, SynchronizedObservableGrouping>.Keys => m_keyDictionary.Keys;

            IEnumerable<SynchronizedObservableGrouping> IReadOnlyDictionary<TKey, SynchronizedObservableGrouping>.Values => m_keyDictionary.Values;
            
            public SynchronizedObservableGrouping this[TKey key] => m_keyDictionary[key];

            IObservableGrouping<TKey, TValue> IReadOnlyList<IObservableGrouping<TKey, TValue>>.this[int index] => this[index];

            #endregion

            #region Public members

            /// <inheritdoc />
            public bool ContainsKey(TKey key) => m_keyDictionary.ContainsKey(key);

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

            public IEnumerable<SynchronizedObservableGrouping> AsEnumerable => this;

            IEnumerator<IObservableGrouping<TKey, TValue>> IEnumerable<IObservableGrouping<TKey, TValue>>.GetEnumerator() => GetEnumerator();

            IEnumerator<KeyValuePair<TKey, SynchronizedObservableGrouping>> IEnumerable<KeyValuePair<TKey, SynchronizedObservableGrouping>>.GetEnumerator() => m_keyDictionary.GetEnumerator();

            bool ICollection.IsSynchronized => true;

            object ICollection.SyncRoot
            {
                get
                {
                    lock (m_valuesCollection)
                        return m_valuesCollection;
                }
            }

            #endregion

            #region Overrides

            protected override void ClearItems()
            {
                m_keyDictionary.Clear();
                base.ClearItems();

                lock (m_valuesCollection)
                {
                    m_valuesCollection.BaseCallCheckin();

                    m_valuesCollection.BaseCallCheckin();
                    m_valuesCollection.Clear();
                    m_valuesCollection.BaseCallCheckout();

                    m_valuesCollection.BaseCallCheckout();
                }
            }

            protected override void InsertItem(int index, SynchronizedObservableGrouping item)
            {
                m_keyDictionary.Add(item.Key, item);
                base.InsertItem(index, item);
            }

            protected override void MoveItem(int oldIndex, int newIndex)
            {
                SynchronizedObservableGrouping shiftedItem = this[newIndex];
                SynchronizedObservableGrouping movedItem = this[oldIndex];

                base.MoveItem(oldIndex, newIndex);

                lock (m_valuesCollection)
                {
                    m_valuesCollection.BaseCallCheckin();

                    for (int i = movedItem.StartIndexInclusive; i < movedItem.EndIndexExclusive; i++)
                    {
                        shiftedItem.EndIndexExclusive++;


                        m_valuesCollection.MoveItem(i, shiftedItem.StartIndexInclusive++);
                    }

                    m_valuesCollection.BaseCallCheckout();
                }
            }

            protected override void SetItem(int index, SynchronizedObservableGrouping item)
            {
                SynchronizedObservableGrouping oldItem = this[index];
                int offset = item.Count - oldItem.Count;
                
                m_keyDictionary.Remove(oldItem.Key);
                m_keyDictionary.Add(item.Key, item);
                base.SetItem(index, item);

                item.EndIndexExclusive = index + item.Count;
                item.StartIndexInclusive = index;
                lock (m_valuesCollection)
                {
                    m_valuesCollection.BaseCallCheckin();

                    m_valuesCollection.OffsetAfterGroup(item, offset);

                    m_valuesCollection.BaseCallCheckout();
                }
            }

            protected override void RemoveItem(int index)
            {
                SynchronizedObservableGrouping item = this[index];
                lock (m_valuesCollection)
                {
                    m_valuesCollection.BaseCallCheckin();

                    m_valuesCollection.OffsetAfterGroup(item, -item.Count);

                    m_keyDictionary.Remove(item.Key);
                    base.RemoveItem(index);

                    for (int i = item.EndIndexExclusive - 1; i >= item.StartIndexInclusive; i--)
                    {
                        m_valuesCollection.RemoveAt(i);
                    }

                    m_valuesCollection.BaseCallCheckout();
                }
            }

            #endregion
        }
    }
}
