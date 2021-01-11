// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
 
using System.Runtime.CompilerServices;
 
// Some routines inspired by the Stanford Bit Twiddling Hacks by Sean Eron Anderson:
// http://graphics.stanford.edu/~seander/bithacks.html
 
namespace System.Numerics
{
    // Source: https://source.dot.net/System.Private.CoreLib/BitOperations.cs.html#https://source.dot.net/System.Private.CoreLib/BitOperations.cs.html,af96b203ee2f79f7

    internal static class BitOperations
    {
        /// <summary>
        /// Rotates the specified value left by the specified number of bits.
        /// Similar in behavior to the x86 instruction ROL.
        /// </summary>
        /// <param name="value">The value to rotate.</param>
        /// <param name="offset">The number of bits to rotate by.
        /// Any value outside the range [0..31] is treated as congruent mod 32.</param>
        /// <returns>The rotated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RotateLeft(uint value, int offset)
            => (value << offset) | (value >> (32 - offset));
    }
}
