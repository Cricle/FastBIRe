using K4os.Hash.xxHash;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Diagnostics.Traces.Mini
{
    [StructLayout(LayoutKind.Sequential, Size = Size)]
    public struct TraceHeader
    {
        public const int UnknowCount = -1;

        public const int Size = 256;

        public long Count;
    }
    public enum TraceCompressMode : byte
    {
        None = 0,
        Zstd = 1
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = HeaderSize)]
    public struct MiniSerializeHeader<TMode>
        where TMode : struct, Enum
    {
        public const int HeaderSize = 13;

        public uint Hash;

        public int Size;

        public TMode Mode;

        public TraceCompressMode CompressMode;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MiniSerializeHeader<TMode> Create(ReadOnlySpan<byte> buffer, TMode mode, TraceCompressMode compressMode)
        {
            return new MiniSerializeHeader<TMode>
            {
                Hash = XXH32.DigestOf(buffer),
                Size = buffer.Length,
                Mode = mode,
                CompressMode = compressMode
            };
        }
    }
}
