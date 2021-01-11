# ObservableGroupCollection&lt;TKey, TValue&gt;

`Namespace: System.Collections.Specialized`

```csharp
public class ObservableGroupCollection<TKey, TValue>
    : ObservableCollection<TValue>, IList<TValue>, ICollection<TValue>, IEnumerable<TValue>, IEnumerable, IList, ICollection, IReadOnlyList<TValue>, IReadOnlyCollection<TValue>, INotifyCollectionChanged, INotifyPropertyChanged, IObservableGroupCollection<TKey, TValue>, IGroupCollection<TKey, TValue>, IObservableCollection<TValue>
```

## Fields

| Type | Name | Summary |
| --- | --- | --- |
| `List<SynchronizedGrouping<TKey, TValue>>` | m_groups |  |
| `IEqualityComparer<TKey>` | m_keyEqualityComparer |  |
| `IDictionary<TKey, SynchronizedGrouping<TKey, TValue>>` | m_syncedGroups |  |

## Properties

| Type | Name | Summary |
| --- | --- | --- |
| `Int32` | GroupCount |  |
| `IObservableGrouping<TKey, TValue>` | Item |  |

## Methods

| Type | Name | Summary |
| --- | --- | --- |
| `void` | AddOrCreate(`TKey&` key, `TValue&` value) |  |
| `void` | AddOrCreate(`IGrouping<TKey, TValue>` grouping) |  |
| `void` | AddOrCreate(`TKey&` key, `IEnumerable<TValue>` values) |  |
| `void` | ClearItems() |  |
| `Boolean` | ContainsKey(`TKey&` key) |  |
| `IObservableGrouping<TKey, TValue>` | Create(`TKey&` key) |  |
| `IEnumerable<IObservableGrouping<TKey, TValue>>` | EnumerateGroupings(`Func<IObservableGrouping<TKey, TValue>, Boolean>` predicate = null) |  |
| `void` | InsertItem(`Int32` index, `TValue` item) |  |
| `void` | InternalAddGroup(`SynchronizedGrouping&` grouping) |  |
| `void` | InternalRemoveGroup(`TKey&` key, `SynchronizedGrouping&` grouping) |  |
| `Boolean` | InternalTryGetGrouping(`TKey&` key, `SynchronizedGrouping&` syncedGrouping) |  |
| `void` | MoveItem(`Int32` oldIndex, `Int32` newIndex) |  |
| `void` | OffsetGroupingsAfter(`SynchronizedGrouping&` emitter, `Int32` itemsCount) |  |
| `Boolean` | Remove(`TKey&` key) |  |
| `void` | RemoveItem(`Int32` index) |  |
| `void` | SetItem(`Int32` index, `TValue` item) |  |
| `Boolean` | TryGetGrouping(`TKey&` key, `IObservableGrouping`2&` grouping) |  |
| `Boolean` | TryGetGrouping(`TKey&` key, `IGrouping`2&` grouping) |  |

---

[`< Back`](../)
