using K4os.Hash.xxHash;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Diagnostics.Traces.Mini
{
    [StructLayout(LayoutKind.Sequential, Size = HeaderSize)]
    public struct TraceHeader
    {
        public const int UnknowCount = -1;

        public const int HeaderSize = 256;

        public long Count;
    }
    [StructLayout(LayoutKind.Sequential, Size = HeaderSize)]
    public struct TraceCounterHeader
    {
        public const int HeaderSize = 64;

        public int FieldCount;
    }
    [StructLayout(LayoutKind.Sequential, Size = HeaderSize)]
    public struct MiniCounterHeader
    {
        public const int HeaderSize = 20;

        public int FieldCount;

        public uint Hash;

        public int Size;

        public DateTime Time;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe MiniCounterHeader Create(DateTime time, ReadOnlySpan<byte> buffer)
        {
            return new MiniCounterHeader
            {
                Hash = XXH32.DigestOf(buffer),
                Size = buffer.Length,
                Time = time
            };
        }
    }
    public enum TraceCompressMode : byte
    {
        None = 0,
        Zstd = 1
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = HeaderSize)]
    public struct MiniBytesStoreHeader
    {
        public const int HeaderSize = 9;

        public uint Hash;

        public int Size;

        public DateTime Time;

        public TraceCompressMode CompressMode;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe MiniBytesStoreHeader Create(DateTime time,ReadOnlySpan<byte> buffer, TraceCompressMode compressMode)
        {
            return new MiniBytesStoreHeader
            {
                Hash = XXH32.DigestOf(buffer),
                Size = buffer.Length,
                CompressMode = compressMode,
                Time=time
            };
        }

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = HeaderSize)]
    public struct MiniDataHeader<TMode>
        where TMode : struct, Enum
    {
        public const int HeaderSize = 13;

        public uint Hash;

        public int Size;

        public TMode Mode;

        public TraceCompressMode CompressMode;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MiniDataHeader<TMode> Create(ReadOnlySpan<byte> buffer, TMode mode, TraceCompressMode compressMode)
        {
            return new MiniDataHeader<TMode>
            {
                Hash = XXH32.DigestOf(buffer),
                Size = buffer.Length,
                Mode = mode,
                CompressMode = compressMode
            };
        }
    }
}
