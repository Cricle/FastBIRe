namespace Diagnostics.Traces.Serialization
{
    public unsafe struct ConstMiniReadSerializer : IMiniReadSerializer
    {
        private int offset;
        private readonly byte* buffer;
        private readonly int bufferLength;

        public ConstMiniReadSerializer(byte* buffer, int bufferLength)
        {
            this.buffer = buffer;
            this.bufferLength = bufferLength;
        }

        public bool CanSeek => true;

        public bool CanRead(int length)
        {
            return bufferLength > length;
        }

        public void Read(Span<byte> buffer)
        {
            if (bufferLength>buffer.Length)
            {
                new Span<byte>((this.buffer+ offset), buffer.Length).CopyTo(buffer);
                offset += buffer.Length;
                return;    
            }
            throw new ArgumentOutOfRangeException($"The buffer size is {bufferLength}, but the buffer is {buffer.Length}");
        }
    }
}
