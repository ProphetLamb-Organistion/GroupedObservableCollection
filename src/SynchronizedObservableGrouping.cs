using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Collections.Specialized
{
    public partial class ObservableGroupCollection<TKey, TValue>
    {
        [DebuggerDisplay("Count = {Count}, Range=[{StartIndexInclusive}..{EndIndexExclusive}), Key = {Key}")]
        public sealed class SynchronizedObservableGrouping
            : IObservableGrouping<TKey, TValue>, ICollection
        {
            #region Fields

            private readonly ObservableGroupCollection<TKey, TValue> _collection;
            private bool _isVerbose = true;
            private int _startIndexInclusive;
            private int _endIndexExclusive;
            private readonly bool _isSorted;

            public event NotifyCollectionChangedEventHandler? CollectionChanged;
            public event PropertyChangedEventHandler? PropertyChanged;

            #endregion

            #region Constructors

            public SynchronizedObservableGrouping(
                in TKey key,
                ObservableGroupCollection<TKey, TValue> collection,
                object syncRoot) :
                this(key, collection.Count, collection.Count, collection, syncRoot)
            { }

            public SynchronizedObservableGrouping(
                in TKey key,
                int startIndexInclusive,
                int endIndexExclusive,
                ObservableGroupCollection<TKey, TValue> collection,
                object syncRoot)
            {
                Key = key;
                _startIndexInclusive = startIndexInclusive;
                _endIndexExclusive = endIndexExclusive;
                _collection = collection ?? throw new ArgumentNullException(nameof(collection));
                _collection.CollectionChanged += CollectionChangedEventSubscriber;
                _isSorted = collection.IsSorted;
                SyncRoot = syncRoot ?? throw new ArgumentNullException(nameof(syncRoot));
            }

            #endregion

            #region Properties
            
            /// <inheritdoc />
            public TKey Key { get; }

            /// <summary>
            /// Gets or sets the index of the first element of the <see cref="SynchronizedObservableGrouping"/> in the synchronized collection.
            /// </summary>
            public int StartIndexInclusive
            {
                get => _startIndexInclusive;
                internal set
                {
                    if ((uint) value > (uint) CollectionCount + 1)
                        throw new IndexOutOfRangeException();
                    _startIndexInclusive = value;
                    OnPropertyChanged();
                }
            }
            
            /// <summary>
            /// Gets or sets the index of the element after the last element of the <see cref="SynchronizedObservableGrouping"/> in the synchronized collection.
            /// </summary>
            public int EndIndexExclusive
            {
                get => _endIndexExclusive;
                internal set
                {
                    if (StartIndexInclusive > value)
                        throw new ArgumentOutOfRangeException(nameof(value), "EndIndexExclusive must be greater or equal to StartIndexInclusive");
                    if ((uint) value > (uint) CollectionCount + 1)
                        throw new IndexOutOfRangeException();
                    _endIndexExclusive = value;
                    OnPropertyChanged();
                }
            }

            /// <inheritdoc cref="IObservableGrouping{TKey,TValue}.Count" />
            public int Count => EndIndexExclusive - StartIndexInclusive;

            /// <inheritdoc />
            public bool IsSynchronized => true;

            /// <inheritdoc />
            public object SyncRoot { get; }

            /// <summary>
            /// Indicates whether the synchronized collection is sorted. If true, can not Insert or Move.
            /// </summary>
            public bool IsSorted => _isSorted; // Do not use auto-property, prevent unnecessary locking

            /// <inheritdoc />
            bool ICollection<TValue>.IsReadOnly => false;
            
            /// <inheritdoc />
            public TValue this[int index]
            {
                get
                {
                    if ((uint)index >= (uint)Count)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    lock (SyncRoot)
                        return _collection[index + StartIndexInclusive];
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
                    lock (SyncRoot)
                        return _collection.Count;
                }   
            }

            #endregion

            #region Public members
            
            /// <inheritdoc />
            public void Add(TValue item)
            {
                lock (SyncRoot)
                    _collection.AddOrCreate(Key, item);

                OnPropertyChanged("Count");
                OnPropertyChanged("Item[]");
            }
            
            /// <inheritdoc />
            public void Insert(int index, TValue item)
            {
                if (IsSorted)
                    throw new NotSupportedException("Can not insert into a sorted collection.");
                if ((uint)index > (uint)Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                if (index == Count)
                {
                    Add(item);
                    return;
                }
                
                lock (SyncRoot)
                {
                    _collection.BaseCallCheckin();

                    _collection.GroupAddValue(this, item, index);

                    _collection.BaseCallCheckout();
                }
                OnPropertyChanged("Count");
                OnPropertyChanged("Item[]");
            }
            
            /// <inheritdoc />
            public void Clear()
            {
                _isVerbose = false;
                lock (SyncRoot)
                {
                    _collection.BaseCallCheckin();

                    for (int i = EndIndexExclusive - 1; i >= StartIndexInclusive; i--)
                    {
                        _collection.RemoveAt(i);
                    }
                    _collection.OffsetAfterGroup(this, -Count);

                    _collection.BaseCallCheckin();
                }
                _isVerbose = true;

                EndIndexExclusive = StartIndexInclusive;
                OnPropertyChanged("Count");
                OnPropertyChanged("Item[]");
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
                    lock (SyncRoot)
                        value = _collection[i];
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
                lock (SyncRoot)
                {
                    _collection.BaseCallCheckin();

                    _collection.RemoveAt(index+StartIndexInclusive);
                    _collection.OffsetAfterGroup(this, -1);

                    _collection.BaseCallCheckout();
                }
                EndIndexExclusive--;
                OnPropertyChanged("Count");
                OnPropertyChanged("Item[]");
            }
            
            /// <inheritdoc />
            public void Move(int oldIndex, int newIndex)
            {
                if (IsSorted)
                    throw new NotSupportedException("Can not move in a sorted collection.");
                lock (SyncRoot)
                {
                    _collection.BaseCallCheckin();

                    _collection.MoveItem(StartIndexInclusive + oldIndex, StartIndexInclusive + newIndex);

                    _collection.BaseCallCheckout();
                }
                OnPropertyChanged("Item[]");
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
                for (int i = StartIndexInclusive; i < EndIndexExclusive; i++)
                {
                    TValue item;
                    lock (SyncRoot)
                        item = _collection[i];
                    array[arrayIndex++] = item;
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
                for (int i = StartIndexInclusive; i < EndIndexExclusive; i++)
                {
                    TValue item;
                    lock (SyncRoot)
                        item = _collection[i];
                    array.SetValue(item, index++);
                }
            }
            
            /// <inheritdoc />
            public IEnumerator<TValue> GetEnumerator()
            {
                for (int i = StartIndexInclusive; i < EndIndexExclusive; i++)
                {
                    TValue item;
                    lock (SyncRoot)
                        item = _collection[i];
                    yield return item;
                }
            }
            
            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            #endregion

            #region Private members

            private void Set(int index, TValue item)
            {
                lock (SyncRoot)
                {
                    _collection.BaseCallCheckin();

                    _collection[index + StartIndexInclusive] = item;

                    _collection.BaseCallCheckout();
                }
                OnPropertyChanged("Item[]");
            }

            public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                if (_isVerbose)
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            
            private void OnCollectionChanged(NotifyCollectionChangedAction action, object? item, int index)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
            }

            private void OnCollectionChanged(NotifyCollectionChangedAction action, object? oldItem, object? item, int index)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, oldItem, item, index));
            }

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

            private void CollectionChangedEventSubscriber(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (!_isVerbose)
                    return;
                if (CollectionChanged is null)
                    return;
                if (!ReferenceEquals(sender, _collection))
                    throw new InvalidOperationException("Sender must be the synchronized collection.");
                int index;
                switch (e!.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        index = e.NewStartingIndex;
                        IEnumerable? newEnumerable;
                        if ((newEnumerable = EnumerateAffectingCollectionChanges(index, e.NewItems)) is null)
                            return;
                        foreach (object obj in newEnumerable)
                            OnCollectionChanged(NotifyCollectionChangedAction.Add, obj, index++);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        index = e.OldStartingIndex;
                        IEnumerable? oldEnumerable;
                        if ((oldEnumerable = EnumerateAffectingCollectionChanges(index, e.OldItems)) is null)
                            return;
                        foreach (object obj in oldEnumerable)
                            OnCollectionChanged(NotifyCollectionChangedAction.Remove, obj, index++);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        index = e.OldStartingIndex;
                        if (EnumerateAffectingCollectionChanges(index, e.OldItems) is null 
                         || EnumerateAffectingCollectionChanges(index, e.NewItems) is null)
                            return;
                        OnCollectionChanged(NotifyCollectionChangedAction.Replace, e.OldItems[0], e.NewItems[0], e.NewStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Move:
                        index = e.OldStartingIndex;
                        if (EnumerateAffectingCollectionChanges(index, e.OldItems) is null)
                            return;
                        OnCollectionChanged(NotifyCollectionChangedAction.Move, e.OldItems, e.NewStartingIndex, e.OldStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        OnCollectionReset();
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