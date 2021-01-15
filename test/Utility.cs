using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GroupedObservableCollection.Test
{
    public static class Utility
    {
        public static void Move<T>(this IList<T> collection, int oldIndex, int newIndex)
        {
            if (collection.IsReadOnly)
                throw new NotSupportedException("Cannot swap items in a ReadOnly-collection.");
            T temp = collection[oldIndex];
            collection.RemoveAt(oldIndex);
            collection.Insert(newIndex, temp);
        }
    }
}
