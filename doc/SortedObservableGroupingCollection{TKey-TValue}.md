# SortedObservableGroupingCollection&lt;TKey, TValue&gt;

`Namespace: System.Collections.Specialized`

```csharp
public class SortedObservableGroupingCollection<TKey, TValue>
    : ObservableGroupingCollection<TKey, TValue>, IList<TValue>, ICollection<TValue>, IEnumerable<TValue>, IEnumerable, IList, ICollection, IReadOnlyList<TValue>, IReadOnlyCollection<TValue>, INotifyCollectionChanged, INotifyPropertyChanged, IObservableGroupingCollection<TKey, TValue>, IGroupingCollection<TKey, TValue>, IObservableCollection<TValue>
```

## Properties

| Type | Name | Summary |
| --- | --- | --- |
| `IComparer<TValue>` | Comparer |  |
| `Boolean` | IsSorted |  |

## Methods

| Type | Name | Summary |
| --- | --- | --- |
| `void` | GroupAdd(`SynchronizedObservableGrouping<TKey, TValue>` grouping, `TValue` item, `Int32` relativeIndex = -1) |  |
| `void` | GroupAdd(`SynchronizedObservableGrouping<TKey, TValue>` grouping, `IReadOnlyList<TValue>` items, `Int32` relativeIndex = -1) |  |
| `void` | SetItem(`Int32` index, `TValue` item) |  |

---

[`< Back`](../README.md)
