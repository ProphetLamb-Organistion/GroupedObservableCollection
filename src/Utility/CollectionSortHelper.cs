using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace GroupedObservableCollection.Utility
{
    internal static class CollectionSortHelper<T>
    {
        // Source: https://source.dot.net/#System.Private.CoreLib/ArraySortHelper.cs,f3d6c6df965a8a86
        // Modified for IReadOnlyList<T> 
        public static int BinarySearch(IReadOnlyList<T> array, int index, int length, T value, IComparer<T>? comparer)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (array.Count < index+length)
                throw new IndexOutOfRangeException();
            comparer ??= Comparer<T>.Default;
            int lo = index;
            int hi = index + length - 1;
            while (lo <= hi)
            {
                int i = lo + ((hi - lo) >> 1);
                int order = comparer.Compare(array[i], value);
                if (order == 0)
                    return i;
                if (order < 0)
                {
                    lo = i + 1;
                }
                else
                {
                    hi = i - 1;
                }
            }

            return ~lo;
        }
    }
}
