﻿using NUnit.Framework;

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
            var col = new ObservableGroupingCollection<KeyStru, ValueClass>();
            var groupings = new Dictionary<KeyStru, IObservableGrouping<KeyStru, ValueClass>>();
            // Add Groupings
            foreach (var grouping in Resources.Instance.EnumerateSampleDataGrouped())
            {
                if (!col.Groupings.TryGetValue(grouping.Key, out var target))
                    target = col.Create(grouping.Key);
                groupings.Add(grouping.Key, target);
            }
            // Add items of grouping in each grouping
            foreach (var (key, value) in Resources.Instance.SampleData)
            {
                groupings[key].Add(value);
            }
            Assert.AreEqual(Resources.Instance.SampleCount, col.Count);
            Assert.AreEqual(Resources.Instance.SampleCount, groupings.Select(x => x.Value.Count).Aggregate((x, y) => x + y));
            Assert.Pass();
        }

        [Test]
        public void Test_Insert()
        {
            var col = new ObservableGroupingCollection<KeyStru, ValueClass>();
            foreach (var grouping in Resources.Instance.EnumerateSampleDataGrouped())
            {
                if (!col.Groupings.TryGetValue(grouping.Key, out var target))
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
            var col = new ObservableGroupingCollection<KeyStru, ValueClass>(Resources.Instance.EnumerateSampleDataGrouped());
            var removedAccumulator = 0;
            foreach (var grouping in col.Groupings.AsEnumerable)
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
            var col = new ObservableGroupingCollection<KeyStru, ValueClass>(Resources.Instance.EnumerateSampleDataGrouped());
            var removedAccumulator = 0;
            foreach (var grouping in col.Groupings.AsEnumerable)
            {
                var removeCount = ThreadLocalRandom.Next(0, grouping.Count);
                for (var i = 0; i < removeCount; i++)
                {
                    grouping.RemoveAt(ThreadLocalRandom.Next(0, grouping.Count));
                    removedAccumulator++;
                }
            }
            Assert.AreEqual(Resources.Instance.SampleCount, removedAccumulator + col.Count);
            Assert.Pass();
        }

        [Test]
        public void Test_Move()
        {
            var col = new ObservableGroupingCollection<KeyStru, ValueClass>(Resources.Instance.EnumerateSampleDataGrouped());
            for (int i = 0; i < col.Groupings.Count; i++)
            {
                col.Groupings.Move(i, ThreadLocalRandom.Next(0, col.Groupings.Count - 1));
            }
            Assert.Pass();
        }
    }
}
