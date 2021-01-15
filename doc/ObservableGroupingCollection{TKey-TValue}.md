# ObservableGroupingCollection&lt;TKey, TValue&gt;

`Namespace: System.Collections.Specialized`

```csharp
public class ObservableGroupingCollection<TKey, TValue>
    : ObservableCollection<TValue>, IList<TValue>, ICollection<TValue>, IEnumerable<TValue>, IEnumerable, IList, ICollection, IReadOnlyList<TValue>, IReadOnlyCollection<TValue>, INotifyCollectionChanged, INotifyPropertyChanged, IObservableGroupingCollection<TKey, TValue>, IGroupingCollection<TKey, TValue>, IObservableCollection<TValue>
```

## Fields

| Type | Name | Summary |
| --- | --- | --- |
| `SynchronizedObservableGroupCollection<TKey, TValue>` | m_groupings |  |

## Properties

| Type | Name | Summary |
| --- | --- | --- |
| `SynchronizedObservableGroupCollection<TKey, TValue>` | Groupings |  |
| `Boolean` | IsSorted |  |
| `SynchronizedObservableGrouping<TKey, TValue>` | Item |  |

## Methods

| Type | Name | Summary |
| --- | --- | --- |
| `void` | Add(`TKey` key, `TValue` value) |  |
| `void` | Add(`IGrouping<TKey, TValue>` grouping) |  |
| `void` | Add(`TKey` key, `IEnumerable<TValue>` values) |  |
| `void` | BaseCallCheckin() |  |
| `void` | BaseCallCheckout() |  |
| `void` | ClearItems() |  |
| `void` | CopyFrom(`IEnumerable<IGrouping<TKey, TValue>>` groupings) |  |
| `SynchronizedObservableGrouping<TKey, TValue>` | Create(`TKey&` key) |  |
| `void` | GroupAdd(`SynchronizedObservableGrouping<TKey, TValue>` grouping, `TValue` item, `Int32` relativeIndex = -1) |  |
| `void` | GroupAdd(`SynchronizedObservableGrouping<TKey, TValue>` grouping, `IReadOnlyList<TValue>` items, `Int32` relativeIndex = -1) |  |
| `SynchronizedObservableGrouping<TKey, TValue>` | GroupingFactory(`TKey` key) |  |
| `void` | InsertItem(`Int32` index, `TValue` item) |  |
| `void` | InsertRange(`Int32` startIndex, `IReadOnlyList<TValue>` items) |  |
| `void` | MoveItem(`Int32` oldIndex, `Int32` newIndex) |  |
| `void` | MoveRange(`Int32` oldIndex, `Int32` newIndex, `Int32` count) |  |
| `Boolean` | Remove(`TKey&` key) |  |
| `void` | RemoveItem(`Int32` index) |  |
| `void` | RemoveRange(`Int32` startIndex, `Int32` count) |  |
| `void` | ThrowOnIllegalBaseCall(`String` callingFunction = null) |  |

## Static Fields

| Type | Name | Summary |
| --- | --- | --- |
| `String` | CountPropertyName |  |
| `String` | GroupingsCountPropertyName |  |
| `String` | IndexerPropertyName |  |

---

[`< Back`](../README.md)
