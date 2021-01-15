# IGroupingCollection&lt;TKey, TValue&gt;

`Namespace: System.Collections.Specialized`

```csharp
public interface IGroupingCollection<TKey, TValue>
    : IReadOnlyList<TValue>, IReadOnlyCollection<TValue>, IEnumerable<TValue>, IEnumerable
```

## Properties

| Type | Name | Summary |
| --- | --- | --- |
| `IReadOnlyList<IGrouping<TKey, TValue>>` | Groupings |  |
| `Boolean` | IsSorted |  |
| `IGrouping<TKey, TValue>` | Item |  |

## Methods

| Type | Name | Summary |
| --- | --- | --- |
| `void` | Add(`TKey` key, `TValue` value) |  |
| `void` | Add(`IGrouping<TKey, TValue>` grouping) |  |
| `void` | Add(`TKey` key, `IEnumerable<TValue>` values) |  |
| `IGrouping<TKey, TValue>` | Create(`TKey&` key) |  |
| `Boolean` | Remove(`TKey&` key) |  |

---

[`< Back`](../README.md)
