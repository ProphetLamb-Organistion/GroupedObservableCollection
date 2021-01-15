using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
                foreach (var grouping in PersonSource.Instance.EnumerateSampleDataGrouped())
                {
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

    }
}
