using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using GroupedObservableCollection;
using GroupedObservableCollection.Import;

namespace System.Collections.Specialized
{
    public partial class ObservableGroupingCollection<TKey, TValue>
    {
        /// <summary>
        /// Represents a class managing the groupings in a <see cref="ObservableGroupingCollection{TKey,TValue}"/>.
        /// </summary>
        [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
        public class SynchronizedSortedObservableGroupCollection
            : SynchronizedObservableGroupCollection
        {
#region Fields

            private readonly IComparer<ISynchronizedObservableGrouping<TKey, TValue>> _wrappedKeyComparer;

#endregion

#region Constructors

            protected internal SynchronizedSortedObservableGroupCollection(
                IEnumerable<ISynchronizedObservableGrouping<TKey, TValue>>? source,
                ObservableGroupingCollection<TKey, TValue> valuesCollection,
                IComparer<TKey> keyComparer)
                : base(valuesCollection)
            {
                Comparer = keyComparer;
                _wrappedKeyComparer = new WrappedGroupingKeyComparer<TKey, TValue>(keyComparer);
                if (source != null)
                    CopyFrom(source);
            }

#endregion

#region Properties

            /// <inheritdoc />
            public override bool IsSorted => true;

            /// <summary>
            /// Returns the instance of the comparer used to compare keys.
            /// The default comparer, if none was provided in the constructor.
            /// </summary>
            public IComparer<TKey> Comparer { get; }

#endregion

#region Public members

            public void CopyFrom(IEnumerable<ISynchronizedObservableGrouping<TKey, TValue>> source)
            {
                foreach (ISynchronizedObservableGrouping<TKey, TValue> grouping in source)
                {
                    InsertItem(Count, grouping);
                }
            }

#endregion

#region Private members

            private void ThrowOnIllegalBaseCall([CallerMemberName] string? callingFunction = null)
            {
                throw new NotSupportedException("The operation \"" + callingFunction + "\" is not supported in SynchronizedSortedObservableGroupCollection.");
            }

            private SynchronizedObservableGrouping GetItem(int index) => (SynchronizedObservableGrouping)Items[index];

#endregion

#region Overrides

            protected override void InsertItem(int index, ISynchronizedObservableGrouping<TKey, TValue> item)
            {
                // Add calls with index = Items.Count, otherwise insertion
                if (index != Items.Count)
                    ThrowOnIllegalBaseCall();
                if (item.Count != 0)
                    throw new ArgumentException("Cannot add a grouping that already contains items.");
                int insertionIndex = ~CollectionSortHelper<ISynchronizedObservableGrouping<TKey, TValue>>.BinarySearch(this, 0, Count, item, _wrappedKeyComparer);
                // Inverts the result, if the search yielded no result -> if we have a result, the key already exists.
                if (insertionIndex < 0)
                    throw new ArgumentException("Key already exists in collection.");
                
                base.InsertItem(insertionIndex, item);
            }

            protected override void MoveItem(int oldIndex, int newIndex)
            {
                ThrowOnIllegalBaseCall();
                base.MoveItem(oldIndex, newIndex);
            }

            protected override void SetItem(int index, ISynchronizedObservableGrouping<TKey, TValue>  item)
            {
                if (!(item is SynchronizedObservableGrouping newItem))
                    throw new ArgumentException("The type of the item must derive from SynchronizedObservableGrouping.");
                SynchronizedObservableGrouping oldItem = GetItem(index);
                int offset = item.Count - oldItem.Count;

                newItem.EndIndexExclusive = index + item.Count;
                newItem.StartIndexInclusive = index;
                lock (m_valuesCollection)
                    m_valuesCollection.Groupings.OffsetAfterGroup(newItem, offset);

                m_keyDictionary.Remove(oldItem.Key);
                m_keyDictionary.Add(item.Key, newItem);
                base.SetItem(index, item);
            }

            protected override void RemoveItem(int index)
            {
                SynchronizedObservableGrouping item = GetItem(index);
                lock (m_valuesCollection)
                {
                    m_valuesCollection.Groupings.OffsetAfterGroup(item, item.Count);
                    m_valuesCollection.RemoveRange(item.StartIndexInclusive, item.Count);

                    m_keyDictionary.Remove(item.Key);
                    base.RemoveItem(index);
                }
            }

#endregion
        }
    }
}
