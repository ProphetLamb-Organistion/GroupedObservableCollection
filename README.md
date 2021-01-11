# Grouped Observalbe Collection

Implementation of ObservableCollection that associates items with IGrouping so that the ObservableGroupCollection can be used in WPF Binding operations while synchronizing each group from a different datasource.

# Features

- [x] Collection operations e.g. Add, Move, Remove
- [x] Group operations e.g. AddOrCreate, Create, Remove, Contains
- [x] Synchronized operations
- [x] ObservableCollection Events
- [ ] Sorting for Keys and Values
- [ ] SerializableAtttribute and ISerializable interface

## Test coverage

- [x] Collection operations e.g. Add, Move, Remove
- [x] Group operations e.g. AddOrCreate, Create, Remove, Contains
- [x] Synchronized operations
- [ ] ObservableCollection Events
- [ ] Sorting for Keys and Values
- [ ] SerializableAtttribute and ISerializable interface

# Documentation

## Class diagram

![Class diagram picture](https://i.imgur.com/SXMDB8W.png)

## Embeded documentation

Documentation generated from xml-documentation

- [`IGroupCollection<TKey, TValue>`](doc/IGroupCollection{TKey-TValue})
- [`IObservableCollection<T>`](doc/IObservableCollection{T})
- [`IObservableGroupCollection<TKey, TValue>`](doc/IObservableGroupCollection{TKey-TValue})
- [`IObservableGrouping<TKey, TValue>`](doc/IObservableGrouping{TKey-TValue})
- [`ObservableGroupCollection<TKey, TValue>`](doc/ObservableGroupCollection{TKey-TValue})
