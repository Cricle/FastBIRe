namespace Diagnostics.Traces.Status
{
    public interface IReadOnlyBufferList<T>:IDisposable
    {
        int Length { get; }

        ReadOnlySpan<T> AsSpan();

        T[] UnsafeGetValues();
    }
}
