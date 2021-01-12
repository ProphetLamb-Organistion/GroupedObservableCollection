using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using GroupedObservableCollection.Import;

namespace System.Collections.Specialized
{
    public partial class ObservableGroupingCollection<TKey, TValue>
    {
        /// <summary>
        /// Represents a class managing the groupings in a <see cref="ObservableGroupingCollection{TKey,TValue}"/>.
        /// </summary>
        [DebuggerDisplay("Count = {Count}")]
        public class SynchronizedSortedObservableGroupCollection
            : SynchronizedObservableGroupCollection
        {
            #region Fields

            private readonly IComparer<SynchronizedObservableGrouping>? _wrappedKeyComparer;

            #endregion

            #region Constructors

            protected internal SynchronizedSortedObservableGroupCollection(
                IEnumerable<SynchronizedObservableGrouping>? source,
                ObservableGroupingCollection<TKey, TValue> valuesCollection,
                IEqualityComparer<TKey>? keyEqualityComparer,
                IComparer<TKey> keyComparer)
                : base(valuesCollection, keyEqualityComparer)
            {
                Comparer = keyComparer;
                _wrappedKeyComparer = Comparer<SynchronizedObservableGrouping>.Create((x, y) => keyComparer.Compare(x.Key, y.Key));
                if (source != null)
                    CopyFrom(source);
            }

            #endregion

            #region Properties

            /// <inheritdoc />
            public override bool IsSorted => true;

            public IComparer<TKey> Comparer { get; }

            #endregion

            #region Public members

            public void CopyFrom(IEnumerable<SynchronizedObservableGrouping> source)
            {
                foreach (SynchronizedObservableGrouping grouping in source)
                {
                     InsertItem(Count, grouping);   
                }
            }

            #endregion

            #region Private members
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void ThrowOnIllegalBaseCall([CallerMemberName] string? callingFunction = null)
            {
                throw new NotSupportedException("The operation \"" + callingFunction + "\" is not supported in SynchronizedSortedObservableGroupCollection.");
            }

            #endregion

            #region Overrides

            protected override void InsertItem(int index, SynchronizedObservableGrouping item)
            {
                // Add calls with index = Items.Count, otherwise insertion
                if (index != Items.Count)
                    ThrowOnIllegalBaseCall();
                int insertionIndex = ~CollectionSortHelper<SynchronizedObservableGrouping>.BinarySearch(this, 0, Count, item, _wrappedKeyComparer);
                // Inverts the result, if the search yielded no result
                if (insertionIndex < 0)
                    throw new ArgumentException("Key already exists in collection.");

                base.InsertItem(insertionIndex, item);
            }

            protected override void MoveItem(int oldIndex, int newIndex)
            {
                ThrowOnIllegalBaseCall();
                base.MoveItem(oldIndex, newIndex);
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
                    m_valuesCollection.Groupings.OffsetAfterGroup(item, offset);
                }
            }

            protected override void RemoveItem(int index)
            {
                SynchronizedObservableGrouping item = this[index];
                lock (m_valuesCollection)
                {
                    m_valuesCollection.Groupings.OffsetAfterGroup(item, item.Count);

                    m_keyDictionary.Remove(item.Key);
                    base.RemoveItem(index);

                    m_valuesCollection.RemoveRange(item.StartIndexInclusive, item.Count);
                }
            }

            #endregion
        }
    }
}
