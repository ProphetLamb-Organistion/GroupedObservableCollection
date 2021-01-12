# IObservableGroupCollection&lt;TKey, TValue&gt;

`Namespace: System.Collections.Specialized`

```csharp
public interface IObservableGroupCollection<TKey, TValue>
    : IGroupCollection<TKey, TValue>, IReadOnlyList<TValue>, IReadOnlyCollection<TValue>, IEnumerable<TValue>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
```

## Properties

| Type | Name | Summary |
| --- | --- | --- |
| `IObservableGrouping<TKey, TValue>` | Item |  |

## Methods

| Type | Name | Summary |
| --- | --- | --- |
| `Boolean` | TryGetGrouping(`TKey&` key, `IObservableGrouping`2&` grouping) |  |

---

[`< Back`](../README.md)
