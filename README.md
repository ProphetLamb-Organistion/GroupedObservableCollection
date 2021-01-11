# Grouped Observalbe Collection

Implementation of ObservableCollection that associates items with IGrouping so that the ObservableGroupCollection can be used in WPF Binding operations while synchronizing each group from a different datasource.

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
  - [ ] Sorting for Keys and Values
  - [ ] SerializableAtttribute and ISerializable interface

### Test coverage

  - [x] Collection operations e.g. Add, Move, Remove
  - [x] Group operations e.g. AddOrCreate, Create, Remove, Contains
  - [x] Synchronized operations
  - [ ] ObservableCollection Events
  - [ ] Sorting for Keys and Values
  - [ ] SerializableAtttribute and ISerializable interface

## Documentation

### Class diagram

![Interface diagram picture](https://i.imgur.com/X5YJBkn.png)
![Class diagram picture](https://i.imgur.com/gLhySIS.png)

### Embeded documentation

Documentation generated from xml-documentation

  - [`IGroupCollection<TKey, TValue>`](doc/IGroupCollection{TKey-TValue}.md)
  - [`IObservableCollection<T>`](doc/IObservableCollection{T}.md)
  - [`IObservableGroupCollection<TKey, TValue>`](doc/IObservableGroupCollection{TKey-TValue}.md)
  - [`IObservableGrouping<TKey, TValue>`](doc/IObservableGrouping{TKey-TValue}.md)
  - [`ObservableGroupCollection<TKey, TValue>`](doc/ObservableGroupCollection{TKey-TValue}.md)
