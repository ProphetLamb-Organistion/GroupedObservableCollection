using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Collections.Specialized
{
    public partial class ObservableGroupingCollection<TKey, TValue>
    {
        [DebuggerDisplay("Count = {" + CountPropertyName + "}, Range=[{" + StartIndexPropertyName + "}..{" + EndIndexPropertyName + "}), Key = [{" + KeyPropertyName + "}]")]
        [Serializable]
        public class SynchronizedObservableGrouping
            : ISynchronizedObservableGrouping<TKey, TValue>, ICollection
        {
            #region Fields

            internal const string CountPropertyName = "Count";
            internal const string IndexerPropertyName = "Item[]";
            internal const string StartIndexPropertyName = "StartIndexInclusive";
            internal const string EndIndexPropertyName = "EndIndexExclusive";
            internal const string KeyPropertyName = "Key";

            protected readonly ObservableGroupingCollection<TKey, TValue> m_collection;
            private int _startIndexInclusive;
            private int _endIndexExclusive;

            [field: NonSerialized]
            private bool _isVerbose = true;
            
            [field: NonSerialized]
            public virtual event NotifyCollectionChangedEventHandler? CollectionChanged;
            
            [field: NonSerialized]
            public virtual event PropertyChangedEventHandler? PropertyChanged;

            #endregion

            #region Constructors

            public SynchronizedObservableGrouping(
                in TKey key,
                ObservableGroupingCollection<TKey, TValue> collection)
                : this(key, collection.Count, collection.Count, collection)
            { }

            public SynchronizedObservableGrouping(
                in TKey key,
                int startIndexInclusive,
                int endIndexExclusive,
                ObservableGroupingCollection<TKey, TValue> collection)
            {
                Key = key;
                _startIndexInclusive = startIndexInclusive;
                _endIndexExclusive = endIndexExclusive;
                SyncRoot = m_collection = collection ?? throw new ArgumentNullException(nameof(collection));
                m_collection.CollectionChanged += ProcessCollectionChanged;
            }

            #endregion

            #region Properties
            
            /// <inheritdoc />
            public TKey Key { get; }

            /// <inheritdoc />
            public int StartIndexInclusive
            {
                get => _startIndexInclusive;
                internal set
                {
                    if (value == _startIndexInclusive)
                        return;
                    if (EndIndexExclusive < value)
                        throw new ArgumentOutOfRangeException(nameof(value), "EndIndexExclusive must be greater or equal to StartIndexInclusive");
                    if ((uint) value > (uint) CollectionCount + 1)
                        throw new IndexOutOfRangeException();
                    _startIndexInclusive = value;

                    OnPropertyChanged();
                    OnPropertyChanged(CountPropertyName);
                    OnPropertyChanged(IndexerPropertyName);
                }
            }
            
            /// <inheritdoc />
            public int EndIndexExclusive
            {
                get => _endIndexExclusive;
                internal set
                {
                    if (value == _endIndexExclusive)
                        return;
                    if ((uint) value > (uint) CollectionCount + 1)
                        throw new IndexOutOfRangeException();
                    _endIndexExclusive = value;
                    
                    OnPropertyChanged();
                    OnPropertyChanged(CountPropertyName);
                    OnPropertyChanged(IndexerPropertyName);
                }
            }

            /// <inheritdoc cref="ISynchronizedObservableGrouping{TKey,TValue}.Count" />
            public int Count => EndIndexExclusive - StartIndexInclusive;

            /// <inheritdoc />
            public bool IsSynchronized => true;

            /// <inheritdoc />
            public object SyncRoot { get; }
            
            /// <inheritdoc />
            public bool IsSorted
            {
                get
                {
                    lock (m_collection)
                        return m_collection.IsSorted;
                }
            }

            /// <inheritdoc />
            bool ICollection<TValue>.IsReadOnly => false;
            
            /// <inheritdoc />
            [IndexerName ("Item")]
            public TValue this[int index]
            {
                get
                {
                    if ((uint)index >= (uint)Count)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    lock (m_collection)
                        return m_collection[index + StartIndexInclusive];
                }
                set
                {
                    if ((uint)index >= (uint)Count)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    Set(index, value);
                }
            }

            private int CollectionCount
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    lock (m_collection)
                        return m_collection.Count;
                }   
            }

            #endregion

            #region Public members
            
            /// <inheritdoc />
            public void Add(TValue item)
            {
                lock (m_collection)
                    m_collection.GroupAdd(this, item);
                
                OnPropertyChanged(CountPropertyName);
                OnPropertyChanged(IndexerPropertyName);
            }
            
            /// <inheritdoc />
            public void Insert(int index, TValue item)
            {
                if (IsSorted)
                    throw new NotSupportedException("Can not insert into a sorted collection.");
                if ((uint)index > (uint)Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                
                lock (m_collection)
                    m_collection.GroupAdd(this, item, index);
                
                OnPropertyChanged(EndIndexPropertyName);
                OnPropertyChanged(CountPropertyName);
                OnPropertyChanged(IndexerPropertyName);
            }
            
            /// <inheritdoc />
            public void Clear()
            {
                _isVerbose = false;
                lock (m_collection)
                {
                    m_collection.RemoveRange(StartIndexInclusive, Count);
                    m_collection.Groupings.OffsetAfterGroup(this, -Count);
                }

                EndIndexExclusive = StartIndexInclusive;
                _isVerbose = true;
                
                OnPropertyChanged(EndIndexPropertyName);
                OnPropertyChanged(StartIndexPropertyName);
                OnPropertyChanged(CountPropertyName);
                OnPropertyChanged(IndexerPropertyName);
                OnCollectionReset();
            }
            
            /// <inheritdoc />
            public bool Contains(TValue item) => IndexOf(item) != -1;

            /// <inheritdoc />
            public bool Remove(TValue item)
            {
                int index = IndexOf(item);
                if (index == -1)
                    return false;
                RemoveAt(index);
                return true;
            }
            
            /// <inheritdoc />
            public int IndexOf(TValue item)
            {
                EqualityComparer<TValue> valueComparer = EqualityComparer<TValue>.Default;
                for (int i = StartIndexInclusive; i < EndIndexExclusive; i++)
                {
                    TValue value;
                    lock (m_collection)
                        value = m_collection[i];
                    if (valueComparer.Equals(item, value))
                        return i - StartIndexInclusive;
                }
                return -1;
            }
            
            /// <inheritdoc />
            public void RemoveAt(int index)
            {
                if ((uint)index >= (uint)Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                lock (m_collection)
                {
                    m_collection.BaseCallCheckin();
                    m_collection.RemoveAt(index + StartIndexInclusive);
                    m_collection.BaseCallCheckout();
                    m_collection.Groupings.OffsetAfterGroup(this, -1);
                }
                EndIndexExclusive--; // Invokes PropertyChanged events
            }

            /// <inheritdoc />
            public void Move(int oldIndex, int newIndex)
            {
                if (IsSorted)
                    throw new NotSupportedException("Can not move in a sorted collection.");
                if ((uint)oldIndex >= (uint)Count)
                    throw new ArgumentOutOfRangeException(nameof(oldIndex));
                if ((uint)newIndex >= (uint)Count)
                    throw new ArgumentOutOfRangeException(nameof(newIndex));
                if (oldIndex == newIndex)
                    return;
                lock (m_collection)
                {
                    m_collection.BaseCallCheckin();
                    m_collection.MoveItem(StartIndexInclusive + oldIndex, StartIndexInclusive + newIndex);
                    m_collection.BaseCallCheckout();
                }
                OnPropertyChanged(IndexerPropertyName);
            }

            /// <inheritdoc />
            public void CopyTo(TValue[] array, int arrayIndex)
            {
                if (array is null)
                    throw new ArgumentNullException(nameof(array));
                if (arrayIndex < 0)
                    throw new ArgumentOutOfRangeException();
                if (array.Length < arrayIndex + Count)
                    throw new IndexOutOfRangeException();
                lock (m_collection)
                {
                    for (int i = StartIndexInclusive; i < EndIndexExclusive; i++)
                    {
                        array[arrayIndex++] = m_collection[i];
                    }
                }
            }

            /// <inheritdoc />
            void ICollection.CopyTo(Array array, int index)
            {
                if (array is null)
                    throw new ArgumentNullException(nameof(array));
                if (index < 0)
                    throw new ArgumentOutOfRangeException();
                if (array.Length < index + Count)
                    throw new IndexOutOfRangeException();
                lock (m_collection)
                {
                    for (int i = StartIndexInclusive; i < EndIndexExclusive; i++)
                    {
                        array.SetValue(m_collection[i], index++);
                    }
                }
            }
            
            /// <inheritdoc />
            public IEnumerator<TValue> GetEnumerator()
            {
                for (int i = StartIndexInclusive; i < EndIndexExclusive; i++)
                {
                    TValue item;
                    lock (m_collection)
                        item = m_collection[i];
                    yield return item;
                }
            }
            
            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            #endregion

            #region Private members

            private void Set(int index, TValue item)
            {
                if (IsSorted)
                    throw new NotSupportedException("Can not assign in a sorted collection.");
                lock (m_collection)
                {
                    m_collection.BaseCallCheckin();
                    m_collection[index + StartIndexInclusive] = item;
                    m_collection.BaseCallCheckout();
                }
                OnPropertyChanged(IndexerPropertyName);
            }

            public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                if (_isVerbose)
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            /// <summary>Use where (action & Add | Remove) != 0</summary>
            private void OnCollectionChanged(NotifyCollectionChangedAction action, object? item, int index)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
            }

            /// <summary>Use where action == Replace</summary>
            private void OnCollectionChanged(NotifyCollectionChangedAction action, object? oldItem, object? item, int index)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, oldItem, item, index));
            }

            /// <summary>Use where action == Move</summary>
            private void OnCollectionChanged(NotifyCollectionChangedAction action, object? item, int index, int oldIndex)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
            }

            private void OnCollectionReset()
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }

            private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                if (_isVerbose)
                    CollectionChanged?.Invoke( this, e);
            }

            protected virtual void ProcessCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                /*
                 * Steps for ProcessCollectionChanged:
                 *
                 * 1) Validate that the values in the args are acceptable.
                 * 2) Translate the indices if necessary.
                 * 3) Raise CollectionChanged.
                 * 4) Raise any PropertyChanged events that apply.
                 *
                 */

                if (!_isVerbose)
                    return;
                if (CollectionChanged is null)
                    return;
                if (!ReferenceEquals(sender, m_collection))
                    throw new InvalidOperationException("Sender must be the synchronized collection.");
                int index;
                switch (e!.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        index = e.NewStartingIndex;
                        IEnumerable? newEnumerable;
                        if ((newEnumerable = EnumerateAffectingCollectionChanges(index, e.NewItems)) is null)
                            break;
                        foreach (object obj in newEnumerable)
                            OnCollectionChanged(NotifyCollectionChangedAction.Add, obj, index++);
                        OnPropertyChanged(EndIndexPropertyName);
                        OnPropertyChanged(IndexerPropertyName);
                        OnPropertyChanged(CountPropertyName);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        index = e.OldStartingIndex;
                        IEnumerable? oldEnumerable;
                        if ((oldEnumerable = EnumerateAffectingCollectionChanges(index, e.OldItems)) is null)
                            break;
                        foreach (object obj in oldEnumerable)
                            OnCollectionChanged(NotifyCollectionChangedAction.Remove, obj, index++);
                        OnPropertyChanged(EndIndexPropertyName);
                        OnPropertyChanged(IndexerPropertyName);
                        OnPropertyChanged(CountPropertyName);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        index = e.OldStartingIndex;
                        if (EnumerateAffectingCollectionChanges(index, e.OldItems) is null 
                         || EnumerateAffectingCollectionChanges(index, e.NewItems) is null)
                            break;
                        OnCollectionChanged(NotifyCollectionChangedAction.Replace, e.OldItems[0], e.NewItems[0], e.NewStartingIndex);
                        OnPropertyChanged(IndexerPropertyName);
                        break;
                    case NotifyCollectionChangedAction.Move:
                        index = e.OldStartingIndex;
                        if (EnumerateAffectingCollectionChanges(index, e.OldItems) is null)
                            break;
                        OnCollectionChanged(NotifyCollectionChangedAction.Move, e.OldItems, e.NewStartingIndex, e.OldStartingIndex);
                        OnPropertyChanged(IndexerPropertyName);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        OnCollectionReset();
                        OnPropertyChanged(EndIndexPropertyName);
                        OnPropertyChanged(IndexerPropertyName);
                        OnPropertyChanged(CountPropertyName);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            private IEnumerable? EnumerateAffectingCollectionChanges(int startIndex, IList items)
            {
                int intersectionStart = Math.Max(StartIndexInclusive - startIndex, 0);
                int intersectionEnd = Math.Min(EndIndexExclusive - startIndex, items.Count);
                if (intersectionStart >= intersectionEnd)
                    return null;
                return EnumerationHelper(items, intersectionStart, intersectionEnd);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static IEnumerable EnumerationHelper(IList items, int start, int end)
            {
                for (int i = start; i < end; i++)
                {
                    yield return items[i];
                }
            }

            #endregion
        }
    }
}