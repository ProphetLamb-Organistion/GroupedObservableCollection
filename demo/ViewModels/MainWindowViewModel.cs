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
        public readonly object PersonsLock = new object();

        public MainWindowViewModel()
        {
            PersonsGroupingCollection = new ObservableGroupingCollection<PersonType, Person>();
            BindingOperations.EnableCollectionSynchronization(Persons, PersonsLock);
        }

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
                if (_selectedGroupIndex is null || value is null)
                    return;
                Groupings.Move(_selectedGroupIndex.Value, value.Value);
                Set(ref _selectedGroupIndex, value);
            }
        }
    }
}
