# ISynchronizedObservableGrouping&lt;TKey, TValue&gt;

`Namespace: System.Collections.Specialized`

```csharp
public interface ISynchronizedObservableGrouping<TKey, TValue>
    : IGrouping<TKey, TValue>, IEnumerable<TValue>, IEnumerable, IList<TValue>, ICollection<TValue>, INotifyCollectionChanged, INotifyPropertyChanged
```

## Properties

| Type | Name | Summary |
| --- | --- | --- |
| `Int32` | EndIndexExclusive |  |
| `Boolean` | IsSorted |  |
| `Int32` | StartIndexInclusive |  |

## Methods

| Type | Name | Summary |
| --- | --- | --- |
| `void` | Move(`Int32` oldIndex, `Int32` newIndex) |  |

---

[`< Back`](../README.md)
