using NUnit.Framework;

using System;
using System.Collections.Specialized;
using System.Linq;

namespace GroupedObservableCollection.Test
{
    public class Test_Collection
    {
        [Test]
        public void Test_Ctor()
        {
            // Simple ctor
            _ = new ObservableGroupingCollection<KeyStru, ValueClass>();
            // Collection ctor
            _ = new ObservableGroupingCollection<KeyStru, ValueClass>(
                Resources.Instance.EnumerateSampleDataGrouped(),
                new KeyStru.EqComp());
            Assert.Pass();
        }
        
        [Test]
        public void Test_AddAutoGroup()
        { 
            IObservableGroupCollection<KeyStru, ValueClass> col = new ObservableGroupingCollection<KeyStru, ValueClass>();
            foreach (var (key, value) in Resources.Instance.SampleData)
            {
                col.AddOrCreate(key, value);
            }
            Assert.AreEqual(Resources.Instance.SampleCount, col.Count);
            Assert.Pass();
        }

        [Test]
        public void Test_AddGroupEnumerable()
        { 
            IObservableGroupCollection<KeyStru, ValueClass> col = new ObservableGroupingCollection<KeyStru, ValueClass>();
            foreach (IGrouping<KeyStru, ValueClass> grouping in Resources.Instance.EnumerateSampleDataGrouped())
            {
                col.AddOrCreate(grouping.Key, grouping);
            }
            Assert.AreEqual(Resources.Instance.SampleCount, col.Count);
            Assert.Pass();
        }

        [Test]
        public void Test_AddGroupSorted()
        {
            IObservableGroupCollection<KeyStru, ValueClass> col = new ObservableGroupingCollection<KeyStru, ValueClass>();
            // Add Groupings
            foreach (IGrouping<KeyStru, ValueClass> grouping in Resources.Instance.EnumerateSampleDataGrouped())
            {
                col.Create(grouping.Key);
            }
            Assert.AreEqual(Resources.Instance.KeyCount, col.GroupCount);
            // Add items
            foreach (var (key, value) in Resources.Instance.SampleData)
            {
                col.AddOrCreate(key, value);
            }
            Assert.AreEqual(Resources.Instance.SampleCount, col.Count);
            Assert.Pass();
        }

        [Test]
        public void Test_AccessGroups()
        {
            IObservableGroupCollection<KeyStru, ValueClass> col = new ObservableGroupingCollection<KeyStru, ValueClass>(
                Resources.Instance.EnumerateSampleDataGrouped(),
                new KeyStru.EqComp());
            int accu = 0;
            foreach (KeyStru key in Resources.Instance.Keys)
            {
                accu += col[key].Count;
            }
            Assert.AreEqual(Resources.Instance.SampleCount, accu);
            Assert.Pass();
        }

        [Test]
        public void Test_RandomTryGetRemove()
        {
            IObservableGroupCollection<KeyStru, ValueClass> col = new ObservableGroupingCollection<KeyStru, ValueClass>(Resources.Instance.EnumerateSampleDataGrouped());
            int removedAccumulator = 0;
            foreach (KeyStru key in Resources.Instance.SampleData.Select(x => x.Key).OrderBy(x => ThreadLocalRandom.Next()))
            {
                if (!col.TryGetGrouping(key, out IObservableGrouping<KeyStru, ValueClass> grouping))
                    continue;
                removedAccumulator += grouping.Count;
                Assert.IsTrue(col.Remove(key));
            }
            Assert.AreEqual(Resources.Instance.SampleCount, removedAccumulator);
            Assert.Pass();
        }

        [Test]
        public void Test_ThrowOnIllegalBaseCall()
        {
            ObservableGroupingCollection<KeyStru, ValueClass> col = new ObservableGroupingCollection<KeyStru, ValueClass>(Resources.Instance.EnumerateSampleDataGrouped());
            Assert.Throws<NotSupportedException>(delegate
            {
                col.Add(ThreadLocalRandom.Choose(Resources.Instance.SampleData).Value);
            });
            Assert.Throws<NotSupportedException>(delegate
            {
                col.Move(76, 531);
            });
            Assert.Throws<NotSupportedException>(delegate
            {
               col.Remove(col[343]); // Consistency do not choose random
            });
            Assert.Throws<NotSupportedException>(delegate
            {
                col.RemoveAt(0);
            });
            Assert.Throws<NotSupportedException>(delegate
            {
                col.Insert(4, ThreadLocalRandom.Choose(Resources.Instance.SampleData).Value);
            });
            Assert.DoesNotThrow(delegate
            {
                col.Clear();
            });
        }
    }
}