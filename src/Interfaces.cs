﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace System.Collections.Specialized
{
    // Microsoft API docs reused as far as possible (https://docs.microsoft.com/en-us/dotnet/api/)
    /// <summary>
    /// Represents a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed. 
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    public interface IReadonlyObservableCollection<out T>
        : IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    { }

    /// <summary>
    /// Represents a mutable collection of items grouped by keys.
    /// </summary>
    /// <typeparam name="TKey">The type of keys.</typeparam>
    /// <typeparam name="TValue">The type of elements in the collection and grouping.</typeparam>
    public interface IGroupCollection<TKey, TValue>
        : IReadOnlyList<TValue>
        where TKey : notnull
    {
        /// <summary>
        /// Gets the number of groups actually contained in the <see cref="IGroupCollection{TKey, TValue}"/>.
        /// </summary>
        int GroupCount { get; }

        /// <summary>
        /// Gets the grouping with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the grouping to get.</param>
        /// <returns>The grouping with the specified <paramref name="key"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="key"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when there is no grouping with the <paramref name="key"/> in the collection.</exception>
        IGrouping<TKey, TValue> this[in TKey key] { get; }

        /// <summary>
        /// Adds the <paramref name="value"/> to the existing grouping with the specified <paramref name="key"/>. If the <paramref name="key"/> does not exist, <see cref="Create"/>s the grouping and adds the <paramref name="value"/>.
        /// </summary>
        /// <param name="key">The key of the targeted grouping.</param>
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
        /// <param name="key">The key of the targeted grouping.</param>
        /// <param name="values">The values to add to the grouping.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="key"/> is null.</exception>
        void AddOrCreate(in TKey key, IEnumerable<TValue> values);

        /// <summary>
        /// Removes the grouping associated with the <paramref name="key"/> from the <see cref="IGroupCollection{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">The key of the grouping to remove from the <see cref="IGroupCollection{TKey, TValue}"/>.</param>
        /// <returns><see cref="true"/> if grouping is successfully removed; otherwise, <see cref="false"/>. This method also returns <see cref="false"/> if grouping was not found in the original <see cref="IGroupCollection{TKey, TValue}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="key"/> is null.</exception>
        bool Remove(in TKey key);

        /// <summary>
        /// Creates an empty grouping with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the grouping to get.</param>
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
    /// Represents a dynamic data collection of items with a common key, that provides notifications when items get added, removed, or when the whole list is refreshed. 
    /// </summary>
    /// <typeparam name="TKey">The type of the common key.</typeparam>
    /// <typeparam name="TValue">The type of elements in the collection.</typeparam>
    public interface IObservableGrouping<out TKey, TValue>
        : IGrouping<TKey, TValue>, IList<TValue>
        where TKey : notnull
    { }

    /// <summary>
    /// Represents a dynamic data collection of items grouped by keys, that provides notifications when items get added, removed, or when the whole list is refreshed.
    /// </summary>
    /// <typeparam name="TKey">The type of keys.</typeparam>
    /// <typeparam name="TValue">The type of elements in the collection.</typeparam>
    public interface IObservableGroupCollection<TKey, TValue>
        : IGroupCollection<TKey, TValue>, IReadonlyObservableCollection<TValue>
        where TKey : notnull
    {
        /// <summary>
        /// Gets the grouping with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the grouping to get.</param>
        /// <returns>The grouping with the specified <paramref name="key"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="key"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when there is no grouping with the <paramref name="key"/> in the collection.</exception>
        new IObservableGrouping<TKey, TValue> this[in TKey key] { get; }

        /// <summary>
        /// Gets the readonly collection representing the groupings of the <see cref="IObservableGroupCollection{TKey,TValue}"/>.
        /// </summary>
        IReadonlyObservableCollection<TKey> Groupings { get; }

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
