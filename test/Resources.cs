using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace GroupedObservableCollection.Test
{
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{Name}")]
    public readonly struct KeyStru : IEquatable<KeyStru>
    {
        public KeyStru(string name, int precedence)
        {
            Name = name;
            Precedence = precedence;
        }

        public readonly string Name;
        public readonly int Precedence;

        public bool Equals(KeyStru other)
        {
            return Name == other.Name && Precedence == other.Precedence;
        }

        public override bool Equals(object obj)
        {
            return obj is KeyStru other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Precedence);
        }

        public static bool operator ==(KeyStru left, KeyStru right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(KeyStru left, KeyStru right)
        {
            return !left.Equals(right);
        }

        public override string ToString() => Name;
    }

    public class KeyStruEqComp : IEqualityComparer<KeyStru>
    {
        public bool Equals(KeyStru x, KeyStru y) => x.Name == y.Name;

        public int GetHashCode(KeyStru obj) => obj.GetHashCode();
    }
    
    [DebuggerDisplay("{Value}")]
    public class ValueClass
    {
        public ValueClass(string value)
        {
            CreationDt = DateTimeOffset.Now;
            Value = value;
        }

        public DateTimeOffset CreationDt { get; set; }
        public string Value { get; set; }
    }

    public class Resources
    {
        public static readonly Lazy<Resources> s_lazy_resources = new Lazy<Resources>(() => new Resources());
        public static Resources Instance => s_lazy_resources.Value;

        public readonly char[] RandomAlphabet = Enumerable.Range(0x20, 0x3A).Concat(Enumerable.Range(0x61, 0x19)).Select(x => (char)x).ToArray();

        public readonly KeyValuePair<KeyStru, ValueClass>[] SampleData;
        public readonly KeyStru[] Keys;
        public readonly int SampleCount = 10000;
        public readonly int KeyCount = (int) Math.Log(10000);

        public Resources()
        {
            SampleData = new KeyValuePair<KeyStru, ValueClass>[SampleCount];
            Keys = new KeyStru[KeyCount];
            for (var i = 0; i < KeyCount; i++)
            {
                Keys[i] = new KeyStru(RandomString(3, 10), ThreadLocalRandom.Next(0, 9));
            }
            for (var i = 0; i < SampleCount; i++)
            {
                SampleData[i] = KeyValuePair.Create(
                    ThreadLocalRandom.Choose(Keys),
                    new ValueClass(RandomString(50, 100)));
            }
        }

        public IEnumerable<IGrouping<KeyStru, ValueClass>> EnumerateSampleDataGrouped() =>
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
