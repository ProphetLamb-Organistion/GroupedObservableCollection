using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Collections.Specialized
{
    public partial class ObservableGroupCollection<TKey, TValue>
    {
        [DebuggerDisplay("Count = {Count}, Range=[{StartIndexInclusive}..{EndIndexExclusive}), Key = {Key}")]
        protected class SynchronizedGrouping
            : IObservableGrouping<TKey, TValue>, ICollection
        {
            #region Fields
            private readonly ObservableGroupCollection<TKey, TValue> _collection;
            private bool _isVerbose = true;
            private readonly object __syncLock;
            private int _startIndexInclusive;
            private int _endIndexExclusive;

            public event NotifyCollectionChangedEventHandler? CollectionChanged;
            public event PropertyChangedEventHandler? PropertyChanged;

            #endregion

            #region Constructors

            public SynchronizedGrouping(in TKey key, ObservableGroupCollection<TKey, TValue> collection, object syncRoot) :
                this(key, collection.Count, collection.Count, collection, syncRoot) 
            { }

            public SynchronizedGrouping(in TKey key, int startIndexInclusive, int endIndexExclusive, ObservableGroupCollection<TKey, TValue> collection, object syncRoot)
            {
                Key = key;
                _startIndexInclusive = startIndexInclusive;
                _endIndexExclusive = endIndexExclusive;
                _collection = collection ?? throw new ArgumentNullException(nameof(collection));
                _collection.CollectionChanged += CollectionChangedEventSubscriber;
                __syncLock = syncRoot ?? throw new ArgumentNullException(nameof(syncRoot));
            }

            #endregion

            #region Properties
            
            /// <inheritdoc />
            public TKey Key { get; }

            /// <summary>
            /// Gets or sets the index of the first element of the <see cref="SynchronizedGrouping"/> in the synchronized collection.
            /// </summary>
            public int StartIndexInclusive
            {
                get => _startIndexInclusive;
                internal set
                {
                    lock (__syncLock)
                    {
                        if ((uint)value > (uint)_collection.Count)
                            throw new IndexOutOfRangeException();
                    }
                    _startIndexInclusive = value;
                    OnPropertyChanged();
                }
            }
            
            /// <summary>
            /// Gets or sets the index of the element after the last element of the <see cref="SynchronizedGrouping"/> in the synchronized collection.
            /// </summary>
            public int EndIndexExclusive
            {
                get => _endIndexExclusive;
                internal set
                {
                    if (StartIndexInclusive > value)
                        throw new ArgumentOutOfRangeException(nameof(value), "EndIndexExclusive must be greater or equal to StartIndexInclusive");
                    lock (__syncLock)
                    {
                        if ((uint)value > (uint)_collection.Count)
                            throw new IndexOutOfRangeException();
                    }
                    _endIndexExclusive = value;
                    OnPropertyChanged();
                }
            }

            /// <inheritdoc cref="IObservableGrouping{TKey,TValue}.Count" />
            public int Count => EndIndexExclusive - StartIndexInclusive;

            /// <inheritdoc />
            public bool IsSynchronized => true;

            /// <inheritdoc />
            public object SyncRoot
            {
                get 
                { 
                    lock(__syncLock)
                        return _collection;
                }
            }
            
            /// <inheritdoc />
            bool ICollection<TValue>.IsReadOnly => false;
            
            /// <inheritdoc />
            public TValue this[int index]
            {
                get
                {
                    if ((uint)index >= (uint)Count)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    lock (__syncLock)
                        return _collection[index + StartIndexInclusive];
                }
                set
                {
                    if ((uint)index >= (uint)Count)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    Set(index, value);
                }
            }

            #endregion

            #region Public members
            
            /// <inheritdoc />
            public void Add(TValue item)
            {
                lock (__syncLock)
                    _collection.AddOrCreate(Key, item);

                OnPropertyChanged("Count");
                OnPropertyChanged("Item[]");
            }
            
            /// <inheritdoc />
            public void Insert(int index, TValue item)
            {
                
                if ((uint)index > (uint)Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                if (index == Count)
                {
                    Add(item);
                    return;
                }
                
                lock (__syncLock)
                {
                    _collection.BaseCallCheckin();

                    _collection.Insert(index + StartIndexInclusive, item);
                    _collection.OffsetGroupingsAfter(this, 1);

                    _collection.BaseCallCheckout();
                }
                EndIndexExclusive++;
                OnPropertyChanged("Count");
                OnPropertyChanged("Item[]");
            }
            
            /// <inheritdoc />
            public void Clear()
            {
                lock (__syncLock)
                {
                    _collection.BaseCallCheckin();

                    for (int i = EndIndexExclusive - 1; i >= StartIndexInclusive; i--)
                    {
                        _collection.RemoveAt(i);
                    }
                    _collection.OffsetGroupingsAfter(this, -Count);

                    _collection.BaseCallCheckin();
                }
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
                    lock (__syncLock)
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
                TValue item = this[index];
                lock (__syncLock)
                {
                    _collection.BaseCallCheckin();

                    _collection.RemoveAt(index+StartIndexInclusive);
                    _collection.OffsetGroupingsAfter(this, -1);

                    _collection.BaseCallCheckout();
                }
                EndIndexExclusive--;
                OnPropertyChanged("Count");
                OnPropertyChanged("Item[]");
            }
            
            /// <inheritdoc />
            public void Move(int oldIndex, int newIndex)
            {
                _isVerbose = false;
                TValue item = this[oldIndex];
                RemoveAt(oldIndex);
                Insert(newIndex, item);
                OnPropertyChanged("Item[]");
                _isVerbose = true;
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
                    lock (_collection)
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
                    lock (_collection)
                        item = _collection[i];
                    array.SetValue(item, index++);
                }
            }
            
            /// <inheritdoc />
            public IEnumerator<TValue> GetEnumerator()
            {
                for (int i = StartIndexInclusive; i < EndIndexExclusive; i++)
                {
                    lock (__syncLock)
                        yield return _collection[i];
                }
            }
            
            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            #endregion

            #region Private members

            private void Set(int index, TValue item)
            {
                lock (__syncLock)
                {
                    _collection.BaseCallCheckin();

                    _collection[index + StartIndexInclusive] = item;

                    _collection.BaseCallCheckout();
                }
                OnPropertyChanged("Item[]");
            }

            public void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                if (_isVerbose)
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            
            private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
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
                        foreach (object obj in EnumerateAffectingCollectionChanges(index, e.NewItems))
                            OnCollectionChanged(NotifyCollectionChangedAction.Add, obj, index++);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                    case NotifyCollectionChangedAction.Remove:
                    case NotifyCollectionChangedAction.Move:
                        index = e.OldStartingIndex;
                        foreach (object obj in EnumerateAffectingCollectionChanges(index, e.OldItems))
                            OnCollectionChanged(NotifyCollectionChangedAction.Add, obj, index++);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        OnCollectionReset();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private IEnumerable EnumerateAffectingCollectionChanges(int startIndex, IList items)
            {
                int exclEnd = startIndex + items.Count;
                // Changes do not affect us
                if (exclEnd <= StartIndexInclusive && startIndex >= EndIndexExclusive)
                    yield break;
                // Intersecting set of changes
                exclEnd = Math.Min(exclEnd, EndIndexExclusive);
                for (int i = Math.Max(startIndex, StartIndexInclusive); i < exclEnd; i++)
                    yield return items[i];
            }

            #endregion
        }
    }
}