using System.Buffers;
using ValueBuffer;

namespace Diagnostics.Traces
{
    public readonly struct GzipCompressResult : IDisposable
    {
        internal readonly ValueBufferMemoryStream stream;
        internal readonly Stream gzipStream;
        internal readonly bool shouldReturn;

        internal GzipCompressResult(ValueBufferMemoryStream stream, Stream gzipStream, bool shouldReturn, byte[] result, int count)
        {
            this.stream = stream;
            this.gzipStream = gzipStream;
            this.shouldReturn = shouldReturn;
            Result = result;
            Count = count;
        }

        public byte[] Result { get; }

        public int Count { get; }

        public Span<byte> Span => new Span<byte>(Result, 0, Count);

        public void Dispose()
        {
            if (shouldReturn)
            {
                ArrayPool<byte>.Shared.Return(Result);
            }
            gzipStream.Dispose();
            stream.Dispose();
        }
    }

}
