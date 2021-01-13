using NUnit.Framework;

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace GroupedObservableCollection.Test
{
    public class Test_Events
    {
        [Test]
        public void Test_AddCollectionEventsOnGroupInvoke()
        {
            var col = new ObservableGroupingCollection<KeyStru, ValueClass>();
            var eventAccumulator = new List<NotifyCollectionChangedEventArgs>();
            col.CollectionChanged += (sender, args) =>
            {
                eventAccumulator.Add(args);
            };
            var groups = Resources.Instance.Keys.Select(key => col.Create(key)).ToArray();

            Assert.AreEqual(0, eventAccumulator.Count);

            foreach (var (key, value) in Resources.Instance.SampleData)
            {
                groups.First(x => x.Key == key).Add(value);
            }

            Assert.AreEqual(0, eventAccumulator.Count(x => x.Action != NotifyCollectionChangedAction.Add));
            Assert.AreEqual(Resources.Instance.SampleCount, eventAccumulator.Select(x => x.NewItems.Count).Aggregate((x, y) => x + y));
            
            Assert.Pass();
        }

        [Test]
        public void Test_AddGroupItemsEvents()
        {
            var col = new ObservableGroupingCollection<KeyStru, ValueClass>();
            var eventAccumulator = new List<NotifyCollectionChangedEventArgs>();

            var groups = Resources.Instance.Keys.Select(key =>
            {
                var g = col.Create(key);
                g.CollectionChanged += (sender, args) => eventAccumulator.Add(args);
                return g;
            }).ToArray();

            foreach (var (key, value) in Resources.Instance.SampleData)
            {
                col.Add(key, value);
            }

            Assert.AreEqual(0, eventAccumulator.Count(x => x.Action != NotifyCollectionChangedAction.Add));
            Assert.AreEqual(Resources.Instance.SampleCount, eventAccumulator.Select(x => x.NewItems.Count).Aggregate((x, y) => x + y));

            Assert.Pass();
        }

        [Test]
        public void Test_MoveGroupItemsEvents()
        {
            var col = new ObservableGroupingCollection<KeyStru, ValueClass>(
                Resources.Instance.EnumerateSampleDataGrouped());
            var eventAccumulator = new List<NotifyCollectionChangedEventArgs>();
            var groups = col.Groupings.AsEnumerable
                .Select(
                    g =>
                    {
                        g.CollectionChanged += (sender, args) => eventAccumulator.Add(args);
                        return g;
                    })
                .ToArray();

            foreach (var g in groups)
            {
                for (var i = 0; i < g.Count; i++)
                {
                    g.Move(i, (i + 1) % (g.Count - 1));
                }
            }

            Assert.AreEqual(0, eventAccumulator.Count(x => x.Action != NotifyCollectionChangedAction.Move));
            Assert.AreEqual(Resources.Instance.SampleCount, eventAccumulator.Select(x => x.OldItems.Count).Aggregate((x, y) => x + y));
        }

        [Test]
        public void Test_RemoveGroupItemsEvents()
        {
            var col = new ObservableGroupingCollection<KeyStru, ValueClass>(Resources.Instance.EnumerateSampleDataGrouped());
            var eventAccumulator = new List<NotifyCollectionChangedEventArgs>();
            var groups = col.Groupings.AsEnumerable.Select(g =>
            {
                g.CollectionChanged += (sender, args) => eventAccumulator.Add(args);
                return g;
            }).ToArray();

            foreach (var grouping in groups)
            {
                for (var i = grouping.Count-1; i >= 0; i--)
                {
                    grouping.RemoveAt(i);
                }
            }
            
            Assert.AreEqual(0, eventAccumulator.Count(x => x.Action != NotifyCollectionChangedAction.Remove));
            Assert.AreEqual(Resources.Instance.SampleCount, eventAccumulator.Select(x => x.OldItems.Count).Aggregate((x, y) => x + y));
            Assert.AreEqual(0, col.Count);
        }

        [Test]
        public void Test_ReplaceGroupItemsEvents()
        {
            var col = new ObservableGroupingCollection<KeyStru, ValueClass>(Resources.Instance.EnumerateSampleDataGrouped());
            var eventAccumulator = new List<NotifyCollectionChangedEventArgs>();
            var groups = col.Groupings.AsEnumerable.Select(g =>
            {
                g.CollectionChanged += (sender, args) => eventAccumulator.Add(args);
                return g;
            }).ToArray();

            for (var i = 0; i < Resources.Instance.SampleCount; i++)
            {
                col[i] = col[ThreadLocalRandom.Next(0, Resources.Instance.SampleCount) % Resources.Instance.SampleCount];
            }
            
            Assert.AreEqual(0, eventAccumulator.Count(x => x.Action != NotifyCollectionChangedAction.Replace));
            Assert.AreEqual(Resources.Instance.SampleCount, eventAccumulator.Select(x => x.OldItems.Count).Aggregate((x, y) => x + y));
            Assert.AreEqual(Resources.Instance.SampleCount, col.Count);
        }

        [Test]
        public void Test_ClearGroupItemsEvents()
        {
            var col = new ObservableGroupingCollection<KeyStru, ValueClass>(Resources.Instance.EnumerateSampleDataGrouped());
            var resetEventAccumulator = new List<NotifyCollectionChangedEventArgs>();
            var groups = col.Groupings.AsEnumerable.Select(g =>
            {
                g.CollectionChanged += (sender, args) => resetEventAccumulator.Add(args);
                return g;
            }).ToArray();

            foreach (var g in groups)
            {
                g.Clear();
            }

            Assert.AreEqual(0, resetEventAccumulator.Count(x => x.Action != NotifyCollectionChangedAction.Reset));
            Assert.AreEqual(groups.Length, resetEventAccumulator.Count(x => x.Action == NotifyCollectionChangedAction.Reset));

            Assert.AreEqual(0, col.Count);
        }

        [Test]
        public void Test_ClearGroupCollectionEvents()
        {
            var col = new ObservableGroupingCollection<KeyStru, ValueClass>(Resources.Instance.EnumerateSampleDataGrouped());
            var removeEventAccumulator = new List<NotifyCollectionChangedEventArgs>();
            var groups = col.Groupings.AsEnumerable.ToArray();
            col.CollectionChanged += (sender, args) => removeEventAccumulator.Add(args);

            foreach (var g in groups)
            {
                g.Clear();
            }

            Assert.AreEqual(0, removeEventAccumulator.Count(x => x.Action != NotifyCollectionChangedAction.Remove));
            Assert.AreEqual(Resources.Instance.SampleCount, removeEventAccumulator.Select(x => x.OldItems.Count).Aggregate((x, y) => x + y));

            Assert.AreEqual(0, col.Count);
        }
    }
}
