using System;
using System.Collections.Generic;
using System.Linq;
using GroupedObservableCollection.Demo.DataModels;

namespace GroupedObservableCollection.Demo.Data
{
    public class PersonSource
    {
        public static readonly Lazy<PersonSource> s_lazy_resources = new Lazy<PersonSource>(() => new PersonSource());
        public static PersonSource Instance => s_lazy_resources.Value;

        public readonly char[] RandomAlphabet = Enumerable.Range(0x20, 0x3A).Concat(Enumerable.Range(0x61, 0x19)).Select(x => (char)x).ToArray();

        public readonly KeyValuePair<PersonType, Person>[] SampleData;
        public readonly PersonType[] Keys;
        public readonly int SampleCount = 1000;
        public readonly int KeyCount = (int)Math.Log(1000);

        public PersonSource()
        {
            SampleData = new KeyValuePair<PersonType, Person>[SampleCount];
            Keys = Enum.GetValues(typeof(PersonType)).Cast<PersonType>().OrderBy(x => ThreadLocalRandom.Next()).ToArray();
            for (var i = 0; i < SampleCount; i++)
            {
                SampleData[i] = KeyValuePair.Create(
                    ThreadLocalRandom.Choose(Keys),
                    new Person
                    {
                        DateOfBirth = DateTimeOffset.FromUnixTimeMilliseconds(ThreadLocalRandom.NextLong(0, DateTimeOffset.Now.ToUnixTimeMilliseconds())),
                        Prename = RandomString(5, 12),
                        Surname = RandomString(5, 12),
                        Type = ThreadLocalRandom.Choose(Keys)
                    });
            }
        }

        public IEnumerable<IGrouping<PersonType, Person>> EnumerateSampleDataGrouped() =>
            SampleData.GroupBy(x => x.Key, x => x.Value);

        private unsafe string RandomString(int minLen, int maxLen)
        {
            var len = ThreadLocalRandom.Next(minLen, maxLen);
            Span<char> buffer = stackalloc char[len];
            ThreadLocalRandom.Fill(buffer, RandomAlphabet);
            return new string(buffer);
        }
    }
}
