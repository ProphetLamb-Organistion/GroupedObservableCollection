using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace System.Collections.Specialized
{
    /*
     * Documentation modified from Microsoft API docs at https://docs.microsoft.com/en-us/dotnet/api/
     */
    /// <summary>
    /// Represents a mutable collection of items grouped by keys.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of elements in the collection and grouping.</typeparam>
    public interface IGroupCollection<TKey, TValue> : IEnumerable<TValue>
    {
        /// <summary>
        /// Gets the number of groups actually contained in the <see cref="IGroupCollection{TKey, TValue}"/>.
        /// </summary>
        int GroupCount { get; }
        /// <summary>
        /// Gets the grouping with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the grouping to get. The value can not be null.</param>
        /// <returns>The grouping with the specified <paramref name="key"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="key"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when there is no grouping with the <paramref name="key"/> in the collection.</exception>
        IGrouping<TKey, TValue> this[in TKey key] { get; }
        /// <summary>
        /// Adds the <paramref name="value"/> to the existing grouping with the specified <paramref name="key"/>. If the <paramref name="key"/> does not exist, <see cref="Create"/>s the grouping and adds the <paramref name="value"/>.
        /// </summary>
        /// <param name="key">The key of the targeted grouping. The value can not be null.</param>
        /// <param name="value">The value to add to the grouping.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="key"/> is null.</exception>
        void AddOrCreate(in TKey key, in TValue value);
        /// <summary>
        /// Adds the <paramref name="grouping"/> to the collection. If a grouping with the same <see cref="IGrouping{TKey,TElement}.Key"/> already exists, adds the <paramref name="grouping"/> to the existing.
        /// </summary>
        /// <param name="grouping">The grouping of values to add to the collection.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <see cref="IGrouping{TKey,TElement}.Key"/> is null.</exception>
        void AddOrCreate(IGrouping<TKey, TValue> grouping);
        /// <summary>
        /// Adds the <paramref name="values"/> to the existing grouping with the specified <paramref name="key"/>. If the <paramref name="key"/> does not exist, <see cref="Create"/>s the grouping and adds the <paramref name="values"/>.
        /// </summary>
        /// <param name="key">The key of the targeted grouping. The value can not be null.</param>
        /// <param name="values">The values to add to the grouping.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="key"/> is null.</exception>
        void AddOrCreate(in TKey key, IEnumerable<TValue> values);
        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="IGroupCollection{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">The key of the grouping to remove from the <see cref="IGroupCollection{TKey, TValue}"/>. The value can not be null.</param>
        /// <returns><see cref="true"/> if item is successfully removed; otherwise, <see cref="false"/>. This method also returns <see cref="false"/> if item was not found in the original <see cref="IGroupCollection{TKey, TValue}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="key"/> is null.</exception>
        bool Remove(in TKey key);
        /// <summary>
        /// Creates an empty grouping with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the grouping to get. The value can not be null.</param>
        /// <returns>The empty grouping created.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when a grouping with the same <paramref name="key"/> already exists.</exception>
        IObservableGrouping<TKey, TValue> Create(in TKey key);
        /// <summary>
        /// Gets the grouping associated with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="grouping">When this method returns, the grouping associated with the specified <paramref name="key"/>, if the <paramref name="key"/> is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns><see cref="true"/> if the <see cref="IGroupCollection{TKey, TValue}"/> contains an grouping with the specified <paramref name="key"/>; otherwise, <see cref="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="key"/> is null.</exception>
        bool TryGetGrouping(in TKey key, out IGrouping<TKey, TValue> grouping);
        /// <summary>
        /// Determines whether the <see cref="IGroupCollection{TKey, TValue}"/> contains an grouping with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="IGroupCollection{TKey, TValue}"/>.</param>
        /// <returns><see cref="true"/> if the <see cref="IGroupCollection{TKey, TValue}"/> contains an grouping with the specified <paramref name="key"/>; otherwise, <see cref="false"/>.</returns>
        bool ContainsKey(in TKey key);
    }
    /// <summary>
    /// Represents a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    public interface IObservableCollection<T>
        : IList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        /// <summary>
        /// Moves the item at the specified index to a new location in the collection.
        /// </summary>
        /// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
        /// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
        /// <remarks>Subclasses can override the MoveItem method to provide custom behavior for this method.</remarks>
        void Move(int oldIndex, int newIndex);
    }
    
    /// <summary>
    /// Represents a dynamic data collection of items with a common key, that provides notifications when items get added, removed, or when the whole list is refreshed. 
    /// </summary>
    /// <typeparam name="TKey">The type of the common key.</typeparam>
    /// <typeparam name="TValue">The type of elements in the collection.</typeparam>
    public interface IObservableGrouping<out TKey, TValue>
        : IObservableCollection<TValue>, IGrouping<TKey, TValue>
    { }

    /// <summary>
    /// Represents a dynamic data collection of items grouped by keys, that provides notifications when items get added, removed, or when the whole list is refreshed.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of elements in the collection.</typeparam>
    public interface IObservableGroupCollection<TKey, TValue>
        : IGroupCollection<TKey, TValue>, IObservableCollection<TValue>
    {
        /// <summary>
        /// Gets the grouping with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the grouping to get. The value can not be null.</param>
        /// <returns>The grouping with the specified <paramref name="key"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="key"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when there is no grouping with the <paramref name="key"/> in the collection.</exception>
        new IObservableGrouping<TKey, TValue> this[in TKey key] { get; }
        /// <summary>
        /// Gets the grouping associated with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="grouping">When this method returns, the grouping associated with the specified <paramref name="key"/>, if the <paramref name="key"/> is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns><see cref="true"/> if the <see cref="IObservableGrouping{TKey, TValue}"/> contains an grouping with the specified <paramref name="key"/>; otherwise, <see cref="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="key"/> is null.</exception>
        bool TryGetGrouping(in TKey key, out IObservableGrouping<TKey, TValue> grouping);
    }
}
