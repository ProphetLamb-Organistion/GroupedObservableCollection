using System;
using System.Collections.Generic;
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
        internal readonly object PersonsLock = new object();

        public MainWindowViewModel()
        {
            Persons = new ObservableGroupingCollection<PersonType, Person>();
            BindingOperations.EnableCollectionSynchronization(Persons, PersonsLock);
            Task.Run(delegate
            {
                foreach (var grouping in PersonSource.Instance.EnumerateSampleDataGrouped())
                {
                    lock (PersonsLock)
                        Persons.Add(grouping);
                }
            });
        }

        public ObservableGroupingCollection<PersonType, Person> Persons { get; }
    }
}
