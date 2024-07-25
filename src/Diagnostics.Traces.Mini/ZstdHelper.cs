using System.Buffers;
using ZstdSharp;

namespace Diagnostics.Traces.Mini
{
    public static class ZstdHelper
    {
        public static ZstdCompressResult WriteZstd(Span<byte> input,int level=0)
        {
            using (var compressor = new Compressor(level))
            {
                var len = Compressor.GetCompressBound(input.Length);
                var buffer = ArrayPool<byte>.Shared.Rent(len);
                var writted = compressor.Wrap(input, buffer);
                return new ZstdCompressResult(buffer, writted);
            }
        }
        public static ZstdCompressResult ReadZstd(ReadOnlySpan<byte> input)
        {
            using (var compressor = new Decompressor())
            {
                var len = Decompressor.GetDecompressedSize(input);
                var buffer = ArrayPool<byte>.Shared.Rent((int)len);
                var writted = compressor.Unwrap(input, buffer);
                return new ZstdCompressResult(buffer, writted);
            }
        }
    }
}
