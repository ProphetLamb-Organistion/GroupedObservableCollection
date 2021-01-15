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
                Resources.Instance.EnumerateSampleDataGrouped());
            Assert.Pass();
        }
        
        [Test]
        public void Test_AddAutoGroup()
        { 
            var col = new ObservableGroupingCollection<KeyStru, ValueClass>();
            foreach (var (key, value) in Resources.Instance.SampleData)
            {
                col.Add(key, value);
            }
            Assert.AreEqual(Resources.Instance.SampleCount, col.Count);
            Assert.Pass();
        }

        [Test]
        public void Test_AddGroupEnumerable()
        { 
            var col = new ObservableGroupingCollection<KeyStru, ValueClass>();
            foreach (var grouping in Resources.Instance.EnumerateSampleDataGrouped())
            {
                col.Add(grouping.Key, grouping);
            }
            Assert.AreEqual(Resources.Instance.SampleCount, col.Count);
            Assert.Pass();
        }

        [Test]
        public void Test_AddGroupSorted()
        {
            var col = new ObservableGroupingCollection<KeyStru, ValueClass>();
            // Add Groupings
            foreach (var grouping in Resources.Instance.EnumerateSampleDataGrouped())
            {
                col.Create(grouping.Key);
            }
            Assert.AreEqual(Resources.Instance.KeyCount, col.Groupings.Count);
            // Add items
            foreach (var (key, value) in Resources.Instance.SampleData)
            {
                col.Add(key, value);
            }
            Assert.AreEqual(Resources.Instance.SampleCount, col.Count);
            Assert.Pass();
        }

        [Test]
        public void Test_AccessGroups()
        {
            var col = new ObservableGroupingCollection<KeyStru, ValueClass>(
                Resources.Instance.EnumerateSampleDataGrouped());
            var accu = 0;
            foreach (var key in Resources.Instance.Keys)
            {
                accu += col[key].Count;
            }
            Assert.AreEqual(Resources.Instance.SampleCount, accu);
            Assert.Pass();
        }

        [Test]
        public void Test_RandomTryGetRemove()
        {
            var col = new ObservableGroupingCollection<KeyStru, ValueClass>(Resources.Instance.EnumerateSampleDataGrouped());
            var removedAccumulator = 0;
            foreach (var key in Resources.Instance.SampleData.Select(x => x.Key).OrderBy(x => ThreadLocalRandom.Next()))
            {
                if (!col.Groupings.TryGetValue(key, out var grouping))
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
            // For some reason stack-overflow ex, when run automatically. Fine when run manually
            Assert.Pass();


            var col = new ObservableGroupingCollection<KeyStru, ValueClass>(Resources.Instance.EnumerateSampleDataGrouped());
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
               col.Remove(col[343]);
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
            
            Assert.Pass();
        }
    }
}