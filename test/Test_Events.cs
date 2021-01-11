using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace GroupedObservableCollection.Test
{
    public class Test_Events
    {
        [Test]
        public void Test_AddCollectionEventsOnGroupInvoke()
        {
            var col = new ObservableGroupCollection<KeyStru, ValueClass>();
            var eventAccumulator = new List<NotifyCollectionChangedEventArgs>();
            // Subscribe
            col.CollectionChanged += (sender, args) =>
            {
                eventAccumulator.Add(args);
            };
            // Create groups
            var groups = Resources.Instance.Keys.Select(key => col.Create(key)).ToArray();

            // Creating groups does not fire any events
            Assert.AreEqual(0, eventAccumulator.Count);

            foreach (var (key, value) in Resources.Instance.SampleData)
            {
                // Add value to group
                groups.First(x => x.Key == key).Add(value);
            }

            // Check if all added items have been submitted
            Assert.AreEqual(Resources.Instance.SampleCount, eventAccumulator.Select(x => x.NewItems.Count).Aggregate((x, y) => x + y));
            
            Assert.Pass();
        }

        [Test]
        public void Test_AddGroupEvents()
        {
            var col = new ObservableGroupCollection<KeyStru, ValueClass>();
            var eventAccumulator = new List<NotifyCollectionChangedEventArgs>();

            // Create groups
            var groups = Resources.Instance.Keys.Select(key =>
            {
                var g = col.Create(key);
                // Subscribe
                g.CollectionChanged += (sender, args) => eventAccumulator.Add(args);
                return g;
            }).ToArray();

            // Fire some events
            foreach (var (key, value) in Resources.Instance.SampleData)
            {
                col.AddOrCreate(key, value);
            }

            // Check if all added items have been submitted
            Assert.AreEqual(Resources.Instance.SampleCount, eventAccumulator.Select(x => x.NewItems.Count).Aggregate((x, y) => x + y));

            Assert.Pass();
        }

        [Test]
        public void Test_MoveGroupEvents()
        {
            var col = new ObservableGroupCollection<KeyStru, ValueClass>(Resources.Instance.EnumerateSampleDataGrouped());
            var eventAccumulator = new List<NotifyCollectionChangedEventArgs>();
            var groups = col.EnumerateGroupings().Select(
                g =>
                {
                    g.CollectionChanged += (sender, args) => eventAccumulator.Add(args);
                    return g;
                }).ToArray();

            for (int i = 0; i < Resources.Instance.SampleCount; i++)
            {
                var g = ThreadLocalRandom.Choose(groups);
                for (int j = 0; j < g.Count; j++, i++)
                {
                    g.Move(j, ThreadLocalRandom.Next(0, g.Count));
                }
            }

            Assert.AreEqual(Resources.Instance.SampleCount, eventAccumulator.Select(x => x.OldItems.Count).Aggregate((x, y) => x + y));
        }
    }
}
