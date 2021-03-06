# Grouped Observalbe Collection

Implementation of `ObservableCollection<T>` that associates items with `IGrouping<TKey, TValue>` so that the `ObservableGroupCollection<TKey, TValue>`  can be used in WPF Binding operations while synchronizing each group from a different datasource.

## Build
| Name | Badge |
| ---- | ---- |
| Travis CI: Build & Test | [![Travis Badge](https://img.shields.io/travis/com/ProphetLamb-Organistion/GroupedObservableCollection)](https://travis-ci.com/github/ProphetLamb-Organistion/GroupedObservableCollection) |
| Codacy: Code quality | [![Codacy Badge](https://app.codacy.com/project/badge/Grade/0bd8fedf894f4625b71e77221dff0976)](https://www.codacy.com/gh/ProphetLamb-Organistion/GroupedObservableCollection/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=ProphetLamb-Organistion/GroupedObservableCollection&amp;utm_campaign=Badge_Grade) |

## Features

  - [x] Collection operations e.g. Add, Move, Remove
  - [x] Group operations e.g. AddOrCreate, Create, Remove, Contains
  - [x] Synchronized operations
  - [x] ObservableCollection Events
  - [x] Sorting for Keys and Values
  - [ ] SerializableAtttribute and ISerializable interface

### Test coverage

  - [x] Collection operations e.g. Add, Move, Remove
  - [x] Group operations e.g. AddOrCreate, Create, Remove, Contains
  - [x] Synchronized operations
  - [X] ObservableCollection Events
  - [ ] Sorting for Keys and Values
  - [ ] SerializableAtttribute and ISerializable interface

## Documentation

### Class diagram

[![Interface diagram picture](https://i.imgur.com/HarPJUN.png)](https://imgur.com/HarPJUN)
[![Class diagram picture](https://i.imgur.com/6l4T4dJ.png)](https://imgur.com/6l4T4dJ)

### Embeded documentation

Documentation generated from xml-documentation

- [`IGroupingCollection<TKey, TValue>`](doc/IGroupingCollection{TKey-TValue}.md)
- [`IObservableCollection<T>`](doc/IObservableCollection{T}.md)
- [`IObservableGroupCollection<TKey, TValue>`](doc/IObservableGroupCollection{TKey-TValue}.md)
- [`IObservableGroupingCollection<TKey, TValue>`](doc/IObservableGroupingCollection{TKey-TValue}.md)
- [`ISynchronizedObservableGrouping<TKey, TValue>`](doc/ISynchronizedObservableGrouping{TKey-TValue}.md)
- [`ObservableGroupingCollection<TKey, TValue>`](doc/ObservableGroupingCollection{TKey-TValue}.md)
- [`SortedObservableGroupingCollection<TKey, TValue>`](doc/SortedObservableGroupingCollection{TKey-TValue}.md)
