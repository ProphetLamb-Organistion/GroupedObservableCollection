// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
 
using System.Security.Cryptography;
using System.Runtime.InteropServices;
 
//Source: https://source.dot.net/System.Private.CoreLib/Interop.GetRandomBytes.cs.html#https://source.dot.net/System.Private.CoreLib/Interop.GetRandomBytes.cs.html,466e9a485f6750aa
internal static partial class Interop
{
    internal class Sys
    {
        [DllImport(Libraries.SystemNative, EntryPoint = "SystemNative_GetNonCryptographicallySecureRandomBytes")]
        internal static extern unsafe void GetNonCryptographicallySecureRandomBytes(byte* buffer, int length);
    }
 
    internal static unsafe void GetRandomBytes(byte* buffer, int length)
    {
        Sys.GetNonCryptographicallySecureRandomBytes(buffer, length);
    }
}

//Source: https://source.dot.net/System.Private.CoreLib/Interop.Libraries.cs.html#https://source.dot.net/System.Private.CoreLib/Interop.Libraries.cs.html,dd531d5f27eb8d23
internal static partial class Interop
{
    internal static class Libraries
    {
        // Shims
        internal const string SystemNative = "libSystem.Native";
    }
}