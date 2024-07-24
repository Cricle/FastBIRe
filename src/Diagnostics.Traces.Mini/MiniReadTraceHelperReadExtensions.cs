using Diagnostics.Traces.Serialization;
using K4os.Hash.xxHash;
using System.Buffers;

namespace Diagnostics.Traces.Mini
{
    public static class MiniReadTraceHelperReadExtensions
    {
        public static MiniSerializeString ReadByteStoreHead(this IMiniReadSerializer serializer)
        {
            return serializer.Read<MiniSerializeString>();
        }
        public unsafe static BytesStoreValue? ReadBytesStoreValue(this IMiniReadSerializer serializer)
        {
            var head = ReadByteStoreHead(serializer);
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
