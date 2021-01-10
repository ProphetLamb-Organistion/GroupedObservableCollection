// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
 
using System.Security.Cryptography;
using System.Runtime.InteropServices;
 
//Source: https://source.dot.net/System.Private.CoreLib/Interop.GetRandomBytes.cs.html#https://source.dot.net/System.Private.CoreLib/Interop.GetRandomBytes.cs.html,466e9a485f6750aa
internal static partial class Interop
{
    internal partial class Sys
    {
        [DllImport(Interop.Libraries.SystemNative, EntryPoint = "SystemNative_GetNonCryptographicallySecureRandomBytes")]
        internal static extern unsafe void GetNonCryptographicallySecureRandomBytes(byte* buffer, int length);
 
        [DllImport(Interop.Libraries.SystemNative, EntryPoint = "SystemNative_GetCryptographicallySecureRandomBytes")]
        internal static extern unsafe int GetCryptographicallySecureRandomBytes(byte* buffer, int length);
    }
 
    internal static unsafe void GetRandomBytes(byte* buffer, int length)
    {
        Sys.GetNonCryptographicallySecureRandomBytes(buffer, length);
    }
 
    internal static unsafe void GetCryptographicallySecureRandomBytes(byte* buffer, int length)
    {
        if (Sys.GetCryptographicallySecureRandomBytes(buffer, length) != 0)
            throw new CryptographicException();
    }
}

//Source: https://source.dot.net/System.Private.CoreLib/Interop.Libraries.cs.html#https://source.dot.net/System.Private.CoreLib/Interop.Libraries.cs.html,dd531d5f27eb8d23
internal static partial class Interop
{
    internal static partial class Libraries
    {
        // Shims
        internal const string SystemNative = "libSystem.Native";
        internal const string NetSecurityNative = "libSystem.Net.Security.Native";
        internal const string CryptoNative = "libSystem.Security.Cryptography.Native.OpenSsl";
        internal const string CompressionNative = "libSystem.IO.Compression.Native";
        internal const string GlobalizationNative = "libSystem.Globalization.Native";
        internal const string IOPortsNative = "libSystem.IO.Ports.Native";
        internal const string Libdl = "libdl";
        internal const string HostPolicy = "libhostpolicy";
    }
}