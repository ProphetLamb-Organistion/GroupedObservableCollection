using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
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

        public void BeginLoadingSampleData()
        {
            Task.Run(delegate
            {
                foreach (var grouping in PersonSource.Instance.EnumerateSampleDataGrouped())
                {
                    lock (PersonsLock)
                        PersonsGroupingCollection.Add(grouping);
                }
            });
        }

        public ObservableGroupingCollection<PersonType, Person> PersonsGroupingCollection { get; }
        public ObservableCollection<Person> Persons => PersonsGroupingCollection;

    }
}
