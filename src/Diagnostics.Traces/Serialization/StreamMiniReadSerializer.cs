namespace Diagnostics.Traces.Serialization
{
    public class StreamMiniReadSerializer : IMiniReadSerializer
    {
        public StreamMiniReadSerializer(Stream stream)
        {
            Stream = stream;
        }

        public Stream Stream { get; }

        public bool? CanRead(int length)
        {
            if (Stream.CanSeek)
            {
                return Stream.Position + length < Stream.Length;
            }
            return null;
        }

        public void Read(Span<byte> buffer)
        {
#if NETSTANDARD2_0 || NET472
            var tmp = new byte[buffer.Length];
            Stream.Read(tmp, 0, tmp.Length);
            tmp.AsSpan().CopyTo(buffer);
#else
            Stream.Read(buffer);
#endif
        }
    }
}
