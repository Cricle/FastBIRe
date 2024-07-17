using System.Buffers;
using System.IO.Compression;
using System.Text;
using ValueBuffer;

namespace Diagnostics.Traces
{
    public static class GzipHelper
    {
        public static GzipCompressResult Compress(string str, Encoding? encoding = null, CompressionLevel level = CompressionLevel.Fastest)
        {
            using (var end = EncodingHelper.SharedEncoding(str, encoding ?? Encoding.UTF8))
            {
                return Compress(end.Buffers, 0, end.Count, level);
            }
        }
        public static GzipCompressResult Compress(byte[] result, int offset, int count, CompressionLevel level = CompressionLevel.Fastest)
        {
            var stream = new ValueBufferMemoryStream();
            try
            {
                var gzip = new DeflateStream(stream, level);
                gzip.Write(result, offset, count);
                gzip.Flush();

                ref ValueList<byte> buffer = ref stream.Buffer;
                var shouldReturn = buffer.BufferSlotIndex != 0;
                var copyBuffer = buffer.DangerousGetArray(0);
                var size = buffer.Size;
                if (shouldReturn)
                {
                    copyBuffer = ArrayPool<byte>.Shared.Rent(size);
                    buffer.ToArray(copyBuffer);
                }
                return new GzipCompressResult(stream, gzip, shouldReturn, copyBuffer, size);
            }
            catch (Exception)
            {
                stream.Dispose();
                throw;
            }
        }
    }

}
