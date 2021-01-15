using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace GroupedObservableCollection.Demo.Data
{
    // Source: https://codeblog.jonskeet.uk/2009/11/04/revisiting-randomness/
    /// <summary>
    /// Convenience class for dealing with randomness.
    /// </summary>
    public static class ThreadLocalRandom
    {
        /// <summary>
        /// Random number generator used to generate seeds,
        /// which are then used to create new random number
        /// generators on a per-thread basis.
        /// </summary>
        private static readonly Random s_globalRandom = new Random();

        private static readonly object s_globalLock = new object();

        /// <summary>
        /// Random number generator
        /// </summary>
        private static readonly ThreadLocal<Random> s_threadRandom = new ThreadLocal<Random>(NewRandom);

        /// <summary>
        /// Creates a new instance of Random. The seed is derived
        /// from a global (static) instance of Random, rather
        /// than time.
        /// </summary>
        public static Random NewRandom()
        {
            lock (s_globalLock)
            {
                return new Random(s_globalRandom.Next());
            }
        }

        /// <summary>
        /// Returns an instance of Random which can be used freely
        /// within the current thread.
        /// </summary>
        public static Random Instance => s_threadRandom.Value!;

        /// <summary>See <see cref="Random.Next()" /></summary>
        public static int Next()
        {
            return Instance.Next();
        }

        /// <inheritdoc cref="Random.Next(Int32)" />
        public static int Next(int maxValue)
        {
            return Instance.Next(maxValue);
        }

        /// <inheritdoc cref="Random.Next(Int32, Int32)" />
        public static int Next(int minValue, int maxValue)
        {
            return Instance.Next(minValue, maxValue);
        }

        public static unsafe long NextLong()
        {
            Span<byte> buffer = new byte[8];
            fixed (byte* bufferPtr = &MemoryMarshal.GetReference(buffer))
            {
                NextBytes(buffer);
                return *(long*)bufferPtr;
            }
        }

        public static long NextLong(long maxValue)
        {
            return NextLong() % maxValue;
        }

        public static long NextLong(long minValue, long maxValue)
        {
            return minValue + NextLong(maxValue);
        }

        /// <summary>See <see cref="Random.NextDouble()" /></summary>
        public static double NextDouble()
        {
            return Instance.NextDouble();
        }
        
        /// <inheritdoc cref="Random.NextBytes(Byte[])" />
        public static void NextBytes(byte[] buffer)
        {
            Instance.NextBytes(buffer);
        }

        /// <inheritdoc cref="Random.NextBytes(Span{Byte})" />
        public static void NextBytes(Span<Byte> buffer)
        {
            Instance.NextBytes(buffer);
        }

        /// <summary>Returns a random boolean value.</summary>
        public static bool NextBool()
        {
            return (Instance.Next() & 1) != 0;
        }

        /// <summary>Return a randomly chosen element of the collection.</summary>
        public static T Choose<T>(IReadOnlyList<T> choices)
        {
            if (choices is null)
                throw new ArgumentNullException(nameof(choices));
            if (choices.Count == 0)
                throw new ArgumentException("No elements in collection.");
            return choices[Next(choices.Count)];
        }
        
        /// <summary>Returns count randomly chosen elements of the collection.</summary>
        public static IEnumerable<T> ChooseMany<T>(IReadOnlyList<T> choices, int count)
        {
            if (choices is null)
                throw new ArgumentNullException(nameof(choices));
            if (choices.Count == 0)
                throw new ArgumentException("No elements in collection.");
            if (count == 0)
                yield break;
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            for (var i = 0; i < count; i++)
                yield return Choose(choices);
        }

        /// <summary>Fills a buffer with randomly chosen elements of alphabet.</summary>
        public static void Fill<T>(Span<T> buffer, ReadOnlySpan<T> alphabet)
        {
            if (alphabet.Length == 0)
                throw new ArgumentException("Collection cannot be empty.", nameof(alphabet));
            if (buffer.Length == 0)
                return;
            for (var i = 0; i < buffer.Length; i++)
                buffer[i] = alphabet[Next(alphabet.Length)];
        }
    }
}
