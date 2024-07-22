using Diagnostics.Traces.Serialization;

namespace Diagnostics.Traces.Mini
{
    internal struct BufferMiniWriteSerializer : IMiniWriteSerializer, IDisposable
    {
        public ArrayPoolBufferWriter<byte> Writer { get; }

        public BufferMiniWriteSerializer()
        {
            Writer = new ArrayPoolBufferWriter<byte>();
        }

        public void Dispose()
        {
            Writer.Dispose();
        }

        public bool Flush()
        {
            return true;
        }

        public void Write(ReadOnlySpan<byte> buffer)
        {
            var sp = Writer.GetSpan(buffer.Length);
            buffer.CopyTo(sp);
            Writer.Advance(buffer.Length);
        }

        public bool CanWrite(int length) => true;
    }
}
