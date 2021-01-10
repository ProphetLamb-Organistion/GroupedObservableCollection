using NUnit.Framework;

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace GroupedObservableCollection.Test
{
    public class Test_Groups
    {
        [Test]
        public void Test_Add()
        {
            IObservableGroupCollection<KeyStru, ValueClass> col = new ObservableGroupCollection<KeyStru, ValueClass>();
            IDictionary<KeyStru, IObservableGrouping<KeyStru, ValueClass>> groupList = new Dictionary<KeyStru, IObservableGrouping<KeyStru, ValueClass>>();
            // Add Groupings
            foreach (IGrouping<KeyStru, ValueClass> grouping in Resources.Instance.EnumerateSampleDataGrouped())
            {
                if (!col.TryGetGrouping(grouping.Key, out var target))
                    target = col.Create(grouping.Key);
                groupList.Add(grouping.Key, target);
            }
            // Add items of grouping in each grouping
            foreach (var (key, value) in Resources.Instance.SampleData)
            {
                groupList[key].Add(value);
            }
            Assert.AreEqual(Resources.Instance.SampleCount, col.Count);
            Assert.AreEqual(Resources.Instance.SampleCount, groupList.Select(x => x.Value.Count).Aggregate((x, y) => x + y));
            Assert.Pass();
        }

        [Test]
        public void Test_Insert()
        {
            IObservableGroupCollection<KeyStru, ValueClass> col = new ObservableGroupCollection<KeyStru, ValueClass>();
            foreach (IGrouping<KeyStru, ValueClass> grouping in Resources.Instance.EnumerateSampleDataGrouped())
            {
                if (!col.TryGetGrouping(grouping.Key, out var target))
                    target = col.Create(grouping.Key);
                foreach(var value in grouping)
                    target.Insert(ThreadLocalRandom.Next(0, target.Count), value);
            }
            Assert.AreEqual(Resources.Instance.SampleCount, col.Count);
            Assert.Pass();
        }

        [Test]
        public void Test_Clear()
        {
            ObservableGroupCollection<KeyStru, ValueClass> col = new ObservableGroupCollection<KeyStru, ValueClass>(Resources.Instance.EnumerateSampleDataGrouped());
            int removedAccumulator = 0;
            foreach (IObservableGrouping<KeyStru, ValueClass> grouping in col.EnumerateGroupings())
            {
                removedAccumulator += grouping.Count;
                grouping.Clear();
            }
            Assert.AreEqual(Resources.Instance.SampleCount, removedAccumulator);
            Assert.AreEqual(0, col.Count);
            Assert.Pass();
        }

        [Test]
        public void Test_Remove()
        {
            ObservableGroupCollection<KeyStru, ValueClass> col = new ObservableGroupCollection<KeyStru, ValueClass>(Resources.Instance.EnumerateSampleDataGrouped());
            int removedAccumulator = 0;
            foreach (IObservableGrouping<KeyStru, ValueClass> grouping in col.EnumerateGroupings())
            {
                int removeCount = ThreadLocalRandom.Next(0, grouping.Count);
                for (int i = 0; i < removeCount; i++)
                {
                    grouping.RemoveAt(ThreadLocalRandom.Next(0, grouping.Count));
                    removedAccumulator++;
                }
            }
            Assert.AreEqual(Resources.Instance.SampleCount, removedAccumulator + col.Count);
            Assert.Pass();
        }
    }
}
