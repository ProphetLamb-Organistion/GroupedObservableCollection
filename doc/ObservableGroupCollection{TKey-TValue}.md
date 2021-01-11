# ObservableGroupCollection&lt;TKey, TValue&gt;

`Namespace: System.Collections.Specialized`

```csharp
public class ObservableGroupCollection<TKey, TValue>
    : ObservableCollection<TValue>, IList<TValue>, ICollection<TValue>, IEnumerable<TValue>, IEnumerable, IList, ICollection, IReadOnlyList<TValue>, IReadOnlyCollection<TValue>, INotifyCollectionChanged, INotifyPropertyChanged, IObservableGroupCollection<TKey, TValue>, IGroupCollection<TKey, TValue>, IObservableCollection<TValue>
```

## Fields

| Type | Name | Summary |
| --- | --- | --- |
| `List<SynchronizedObservableGrouping<TKey, TValue>>` | m_groups |  |
| `IEqualityComparer<TKey>` | m_keyEqualityComparer |  |
| `IDictionary<TKey, SynchronizedObservableGrouping<TKey, TValue>>` | m_syncedGroups |  |

## Properties

| Type | Name | Summary |
| --- | --- | --- |
| `Int32` | GroupCount |  |
| `Boolean` | IsSorted |  |
| `SynchronizedObservableGrouping<TKey, TValue>` | Item |  |

## Methods

| Type | Name | Summary |
| --- | --- | --- |
| `void` | AddOrCreate(`TKey&` key, `TValue&` value) |  |
| `void` | AddOrCreate(`IGrouping<TKey, TValue>` grouping) |  |
| `void` | AddOrCreate(`TKey&` key, `IEnumerable<TValue>` values) |  |
| `void` | BaseCallCheckin() |  |
| `void` | BaseCallCheckout() |  |
| `void` | ClearItems() |  |
| `Boolean` | ContainsKey(`TKey&` key) |  |
| `SynchronizedObservableGrouping<TKey, TValue>` | Create(`TKey&` key) |  |
| `IEnumerable<IObservableGrouping<TKey, TValue>>` | EnumerateGroupings(`Func<IObservableGrouping<TKey, TValue>, Boolean>` predicate = null) |  |
| `void` | GroupAdd(`SynchronizedObservableGrouping&` observableGrouping) |  |
| `void` | GroupAddValue(`SynchronizedObservableGrouping<TKey, TValue>` group, `TValue` item, `Int32` desiredIndex = -1, `Boolean` offset = True) |  |
| `void` | GroupRemove(`SynchronizedObservableGrouping&` observableGrouping) |  |
| `Boolean` | GroupTryGet(`TKey&` key, `SynchronizedObservableGrouping&` syncedObservableGrouping) |  |
| `void` | InsertItem(`Int32` index, `TValue` item) |  |
| `void` | MoveItem(`Int32` oldIndex, `Int32` newIndex) |  |
| `Boolean` | Remove(`TKey&` key) |  |
| `void` | RemoveItem(`Int32` index) |  |
| `void` | SetItem(`Int32` index, `TValue` item) |  |
| `void` | ThrowOnIllegalBaseCall(`String` callingFunction = null) |  |
| `Boolean` | TryGetGrouping(`TKey&` key, `IObservableGrouping`2&` grouping) |  |
| `Boolean` | TryGetGrouping(`TKey&` key, `SynchronizedObservableGrouping&` observableGrouping) |  |

---

[`< Back`](../README.md)
