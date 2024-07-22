using K4os.Hash.xxHash;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Diagnostics.Traces.Mini
{
    [StructLayout(LayoutKind.Sequential, Size = Size)]
    public struct TraceHeader
    {
        public const int Size = 256;

        public long Count;
    }

    [StructLayout(LayoutKind.Sequential, Size = HeaderSize)]
    public struct MiniSerializeHeader<TMode>
        where TMode : struct, Enum
    {
        public const int HeaderSize = 12;

        public uint Hash;

        public int Size;

        public TMode Mode;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MiniSerializeHeader<TMode> Create(ReadOnlySpan<byte> buffer, TMode mode)
        {
            return new MiniSerializeHeader<TMode>
            {
                Hash = XXH32.DigestOf(buffer),
                Size = buffer.Length,
                Mode = mode
            };
        }
    }
}
