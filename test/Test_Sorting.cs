using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace GroupedObservableCollection.Test
{
    public class Test_Sorting
    {
        [Test]
        public void Test_ValueSortedCollectionAdd()
        {
            var col = new SortedObservableGroupingCollection<KeyStru, ValueClass>(KeyComparer, ValueComparer);

            foreach (var (key, value) in Resources.Instance.SampleData)
            {
                col.Add(key, value);
            }

            Assert.AreEqual(Resources.Instance.SampleCount, col.Count);
            Assert.AreEqual(Resources.Instance.SampleCount, col.Groupings.AsEnumerable.Select(x => x.Count).Aggregate((x, y) => x + y));
        }

        [Test]
        public void Test_ValueSortedGroupAdd()
        {
            var col = new SortedObservableGroupingCollection<KeyStru, ValueClass>(KeyComparer, ValueComparer);
            var groups = Resources.Instance.EnumerateSampleDataGrouped().Select(x => col.Create(x.Key)).ToArray();

            foreach (var (key, value) in Resources.Instance.SampleData)
            {
                groups.First(x => x.Key == key).Add(value);
            }

            Assert.IsTrue(col.IsSorted);

            Assert.AreEqual(Resources.Instance.SampleCount, col.Count);
            Assert.AreEqual(Resources.Instance.SampleCount, col.Groupings.AsEnumerable.Select(x => x.Count).Aggregate((x, y) => x + y));
        }

        [Test]
        public void Test_ThrowsOnInsert()
        {
            var col = new SortedObservableGroupingCollection<KeyStru, ValueClass>(Resources.Instance.EnumerateSampleDataGrouped(), KeyComparer, ValueComparer);

            Assert.IsTrue(col.IsSorted);

            Assert.Throws<NotSupportedException>(delegate
            {
                col.Insert(ThreadLocalRandom.Next(0, col.Count-1), ThreadLocalRandom.Choose(Resources.Instance.SampleData).Value);
            });

            Assert.Throws<NotSupportedException>(delegate
            {
                var (key, value) = ThreadLocalRandom.Choose(Resources.Instance.SampleData);
                var g = col.Groupings.AsEnumerable.First(x => x.Key == key);
                Assert.IsTrue(g.IsSorted);
                g.Insert(ThreadLocalRandom.Next(0, g.Count-1), value);
            });
        }

        [Test]
        public void Test_ThrowsOnAssign()
        {
            var col = new SortedObservableGroupingCollection<KeyStru, ValueClass>(Resources.Instance.EnumerateSampleDataGrouped(), KeyComparer, ValueComparer);

            Assert.IsTrue(col.IsSorted);

            Assert.Throws<NotSupportedException>(delegate
            {
                col[ThreadLocalRandom.Next(0, col.Count - 1)] = ThreadLocalRandom.Choose(Resources.Instance.SampleData).Value;
            });

            Assert.Throws<NotSupportedException>(delegate
            {
                var (key, value) = ThreadLocalRandom.Choose(Resources.Instance.SampleData);
                var g = col.Groupings.AsEnumerable.First(x => x.Key == key);
                Assert.IsTrue(g.IsSorted);
                g[ThreadLocalRandom.Next(0, g.Count - 1)] = ThreadLocalRandom.Choose(Resources.Instance.SampleData).Value;
            });
        }

        private IComparer<ValueClass> ValueComparer => Comparer<ValueClass>.Create((x, y) =>
        {
            var dt = x.CreationDt.CompareTo(y.CreationDt);
            return dt != 0 ? dt : String.Compare(x.Value, y.Value, StringComparison.Ordinal);
        });

        private IComparer<KeyStru> KeyComparer => Comparer<KeyStru>.Create((x, y) =>
        {
            var prec = x.Precedence.CompareTo(y.Precedence);
            return prec != 0 ? prec : String.Compare(x.Name, y.Name, StringComparison.Ordinal);
        });
    }
}
