# SortedObservableGroupCollection&lt;TKey, TValue&gt;

`Namespace: System.Collections.Specialized`

```csharp
public class SortedObservableGroupCollection<TKey, TValue>
    : ObservableGroupCollection<TKey, TValue>, IList<TValue>, ICollection<TValue>, IEnumerable<TValue>, IEnumerable, IList, ICollection, IReadOnlyList<TValue>, IReadOnlyCollection<TValue>, INotifyCollectionChanged, INotifyPropertyChanged, IObservableGroupCollection<TKey, TValue>, IGroupCollection<TKey, TValue>, IObservableCollection<TValue>
```

## Properties

| Type | Name | Summary |
| --- | --- | --- |
| `Boolean` | IsSorted |  |
| `IComparer<TKey>` | KeyComparer |  |
| `IComparer<TValue>` | ValueComparer |  |

## Methods

| Type | Name | Summary |
| --- | --- | --- |
| `void` | GroupAdd(`SynchronizedObservableGrouping&` observableGrouping) |  |
| `void` | GroupAddValue(`SynchronizedObservableGrouping<TKey, TValue>` group, `TValue` item, `Int32` desiredIndex = -1, `Boolean` offset = True) |  |

---

[`< Back`](../README.md)
