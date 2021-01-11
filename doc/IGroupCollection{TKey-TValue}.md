# IGroupCollection&lt;TKey, TValue&gt;

`Namespace: System.Collections.Specialized`

```csharp
public interface IGroupCollection<TKey, TValue>
    : IEnumerable<TValue>, IEnumerable
```

## Properties

| Type | Name | Summary |
| --- | --- | --- |
| `Int32` | GroupCount |  |
| `IGrouping<TKey, TValue>` | Item |  |

## Methods

| Type | Name | Summary |
| --- | --- | --- |
| `void` | AddOrCreate(`TKey&` key, `TValue&` value) |  |
| `void` | AddOrCreate(`IGrouping<TKey, TValue>` grouping) |  |
| `void` | AddOrCreate(`TKey&` key, `IEnumerable<TValue>` values) |  |
| `Boolean` | ContainsKey(`TKey&` key) |  |
| `IObservableGrouping<TKey, TValue>` | Create(`TKey&` key) |  |
| `Boolean` | Remove(`TKey&` key) |  |
| `Boolean` | TryGetGrouping(`TKey&` key, `IGrouping`2&` grouping) |  |

---

[`< Back`](../)
