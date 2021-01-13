using System;
using System.Collections.Generic;
using System.Text;

namespace GroupedObservableCollection.Demo.DataModels
{
    internal static class Helpers
    {
        public static PersonType KeySelector(this Person p) => p.Type;
    }
}
