using Diagnostics.Traces.Serialization;
using K4os.Hash.xxHash;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Diagnostics.Traces.Mini
{
    public static class MiniReadTraceHelperReadExtensions
    {
        public static MiniBytesStoreHeader ReadByteStoreValueHeader(this IMiniReadSerializer serializer)
        {
            return serializer.Read<MiniBytesStoreHeader>();
        }
        public static MiniCounterHeader ReadCounterValueHeader(this IMiniReadSerializer serializer)
        {
            return serializer.Read<MiniCounterHeader>();
        }
        public unsafe static CounterValue? ReadCounterValue(this IMiniReadSerializer serializer)
        {
            var head = ReadCounterValueHeader(serializer);
            if (head.Hash == 0 || head.Size % sizeof(double) != 0)
            {
                return null;
            }
            var buffer = ArrayPool<byte>.Shared.Rent(head.Size);
            try
            {
                var sp = buffer.AsSpan(0, head.Size);
                serializer.Read(sp);
                if (head.Hash != XXH32.DigestOf(sp))
                {
                    return null;
                }
                var counterCount = head.Size / sizeof(double);
                var counterValues = new double?[counterCount];
                byte* ptr = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(sp));
                for (int i = 0; i < counterCount; i++)
                {
                    var val = Unsafe.Read<double>(ptr + i * sizeof(double));
                    if (!double.IsNaN(val))
                    {
                        counterValues[i] = val;
                    }
                }
                return new CounterValue(head.Time, counterValues);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
        public unsafe static BytesStoreValue? ReadBytesStoreValue(this IMiniReadSerializer serializer)
        {
            var head = ReadByteStoreValueHeader(serializer);
            if (head.Hash == 0)
            {
                return null;
            }
            var buffer = ArrayPool<byte>.Shared.Rent(head.Size);
            try
            {
                var sp = buffer.AsSpan(0, head.Size);
                serializer.Read(sp);
                if (head.Hash != XXH32.DigestOf(sp))
                {
                    return null;
                }
                if (head.CompressMode == TraceCompressMode.None)
                {
                    return new BytesStoreValue(head.Time, sp.ToArray());
                }
                using (var zstdResult = ZstdHelper.ReadZstd(sp))
                {
                    return new BytesStoreValue(head.Time, zstdResult.Span.ToArray());
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
