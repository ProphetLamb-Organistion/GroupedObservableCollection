using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;

using GroupedObservableCollection.Demo.Data;
using GroupedObservableCollection.Demo.DataModels;

namespace GroupedObservableCollection.Demo.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
#region Fields

        public readonly object PersonsLock = new object();

#endregion

#region Constructors

        public MainWindowViewModel()
        {
            PersonsGroupingCollection = new ObservableGroupingCollection<PersonType, Person>();
            BindingOperations.EnableCollectionSynchronization(Persons, PersonsLock);
        }

#endregion

#region Properties

        public ObservableGroupingCollection<PersonType, Person> PersonsGroupingCollection { get; }
        public ObservableCollection<Person> Persons => PersonsGroupingCollection;
        public ObservableCollection<ISynchronizedObservableGrouping<PersonType, Person>> Groupings => PersonsGroupingCollection.Groupings;

        private PersonType _newPersonType = PersonType.Unknown;
        public PersonType NewPersonType
        {
            get => _newPersonType;
            set => Set(ref _newPersonType, value);
        }

        private string _newPersonPrename = String.Empty;
        public string NewPersonPrename
        {
            get => _newPersonPrename;
            set => Set(ref _newPersonPrename, value);
        }

        private string _newPersonSurname = String.Empty;
        public string NewPersonSurname
        {
            get => _newPersonSurname;
            set => Set(ref _newPersonSurname, value);
        }

        private DateTime? _newPersonDateOfBirth = DateTime.Now.AddYears(-40);
        public DateTime? NewPersonDateOfBirth
        {
            get => _newPersonDateOfBirth;
            set => Set(ref _newPersonDateOfBirth, value);
        }

        private ISynchronizedObservableGrouping<PersonType, Person>? _selectedGroup;
        public ISynchronizedObservableGrouping<PersonType, Person>? SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                Set(ref _selectedGroup, value);
                int index;
                if (!(value is null) && (index = Groupings.IndexOf(value)) != -1)
                    Set(ref _selectedGroupIndex, index, nameof(SelectedGroupIndex));
                else
                    Set(ref _selectedGroup, null, nameof(SelectedGroupIndex));
            }
        }

        private int? _selectedGroupIndex;
        public int? SelectedGroupIndex
        {
            get => _selectedGroupIndex;
            set
            {
                if (!SelectedGroupIndex.HasValue || !value.HasValue)
                    return;
                Groupings.Move(SelectedGroupIndex.Value, value.Value);
                Set(ref _selectedGroupIndex, value);
                SelectedPerson = null;
            }
        }

        private Person? _selectedPerson;
        public Person? SelectedPerson
        {
            get => _selectedPerson;
            set
            {
                if (value is null)
                {
                    Set(ref _selectedPersonGroup, null, nameof(SelectedPersonGroup));
                    Set(ref _selectedPersonIndexInGroup, null, nameof(SelectedPersonIndexInGroup));
                    Set(ref _selectedPerson);
                }
                else
                {
                    ISynchronizedObservableGrouping<PersonType, Person>? group;
                    lock (PersonsLock)
                    {
                        int itemIndex = PersonsGroupingCollection.IndexOf(value);
                        group = Groupings
                           .FirstOrDefault(x => x.StartIndexInclusive <= itemIndex && x.EndIndexExclusive > itemIndex);
                    }

                    if (group is null)
                        throw new InvalidOperationException("The value does not belong to any grouping.");
                    Set(ref _selectedPersonGroup, group, nameof(SelectedPersonGroup));
                    Set(ref _selectedPersonIndexInGroup, group.IndexOf(value), nameof(SelectedPersonIndexInGroup));
                    Set(ref _selectedPerson, value);
                }
            }
        }

        private int? _selectedPersonIndexInGroup;
        public int? SelectedPersonIndexInGroup
        {
            get => _selectedPersonIndexInGroup;
            set
            {
                if (!SelectedPersonIndexInGroup.HasValue || SelectedPersonGroup is null || !value.HasValue)
                    return;
                SelectedPersonGroup.Move(SelectedPersonIndexInGroup.Value, value.Value);
                SelectedPerson = SelectedPersonGroup[value.Value];
            }
        }

        private ISynchronizedObservableGrouping<PersonType, Person>? _selectedPersonGroup;
        public ISynchronizedObservableGrouping<PersonType, Person>? SelectedPersonGroup
        {
            get => _selectedPersonGroup;
            set => Set(ref _selectedPersonGroup, value);
        }

        #endregion

#region Public members

        public void BeginLoadingSampleData(Func<Action, DispatcherOperation> dispatch)
        {
            Task.Run(delegate
            {
                foreach (var grouping in PersonSource.Instance.SampleData.GroupBy(x => x.Value.Type, x => x.Value))
                {
                    Debug.Assert(grouping.Count(x => x.Type != grouping.Key) == 0, "grouping.Count(x => x.Type != grouping.Key) == 0");
                    dispatch(() =>
                    {
                        lock (PersonsLock)
                        {
                            PersonsGroupingCollection.Add(grouping);
                        }
                    });
                }
            });
        }

        public void RemoveSelectedGroup()
        {
            if (!SelectedGroupIndex.HasValue)
                return;
            lock (PersonsLock)
            {
                if (SelectedPersonGroup == Groupings[SelectedGroupIndex.Value])
                    SelectedPerson = null;
                Groupings.RemoveAt(SelectedGroupIndex.Value);
            }
        }

        public Person AddNewPerson()
        {
            var p = new Person
            {
                Prename = NewPersonPrename,
                Surname = NewPersonSurname,
                DateOfBirth = new DateTimeOffset(NewPersonDateOfBirth ?? DateTime.MinValue),
                Type = NewPersonType
            };
            lock (PersonsLock)
                PersonsGroupingCollection.Add(p.Type, p);
            return p;
        }

#endregion
    }
}
