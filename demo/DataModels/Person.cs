using System;
using System.Collections.Generic;
using System.Text;

namespace GroupedObservableCollection.Demo.DataModels
{
    public class Person
    {
        public PersonType Type { get; set; }

        public string? Prename { get; set; }

        public string? Surname { get; set; }

        public DateTimeOffset DateOfBirth { get; set; }
    }
}
