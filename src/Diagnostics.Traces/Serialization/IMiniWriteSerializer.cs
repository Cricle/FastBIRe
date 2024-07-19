namespace Diagnostics.Traces.Serialization
{
    public interface IMiniWriteSerializer
    {
        void Write(ReadOnlySpan<byte> buffer);

        bool Flush();
    }
    public interface IMiniReadSerializer
    {
        bool? CanRead(int length);

        void Read(Span<byte> buffer);
    }
}
