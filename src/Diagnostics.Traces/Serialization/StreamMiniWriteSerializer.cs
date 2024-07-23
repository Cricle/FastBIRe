using System.Buffers;

namespace Diagnostics.Traces.Serialization
{
    public class StreamMiniWriteSerializer : BufferMiniWriteSerializer
    {
        public StreamMiniWriteSerializer(Stream stream)
        {
            Stream = stream;
        }

        public Stream Stream { get; }
        
        protected override void WriteCore(ReadOnlySpan<byte> buffer)
        {
#if NETSTANDARD2_0 || NET472
            var copy = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                buffer.CopyTo(copy);
                Stream.Write(copy, 0, buffer.Length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(copy);
            }
#else
            Stream.Write(buffer);
#endif
        }

        public override bool CanWrite(int length) => true;

    }
}
