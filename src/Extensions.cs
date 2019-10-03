using System;
using System.Runtime.InteropServices;

namespace Victoria
{
    internal static class Extensions
    {
        public static ReadOnlySpan<byte> ToBytes(this ReadOnlySpan<char> str)
            => MemoryMarshal.Cast<char, byte>(str);

        public static bool Match(this ReadOnlySpan<char> str, ReadOnlySpan<byte> bytes)
            => ToBytes(str)
                .SequenceEqual(bytes);
    }
}