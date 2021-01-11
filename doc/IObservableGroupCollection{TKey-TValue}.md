# IObservableGroupCollection&lt;TKey, TValue&gt;

`Namespace: System.Collections.Specialized`

```csharp
public interface IObservableGroupCollection<TKey, TValue>
    : IGroupCollection<TKey, TValue>, IEnumerable<TValue>, IEnumerable, IObservableCollection<TValue>, IList<TValue>, ICollection<TValue>, INotifyCollectionChanged, INotifyPropertyChanged
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
