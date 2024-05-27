namespace Diagnostics.Traces.Status
{
    public interface IStatusScope : IAsyncDisposable,IDisposable
    {
        string Name { get; }

        bool IsComplated { get; }

        bool Set(string status);

        Task<bool> SetAsync(string status, CancellationToken token = default);

        bool Log(string message);

        Task<bool> LogAsync(string message, CancellationToken token = default);

        bool Complate(StatuTypes types = StatuTypes.Unset);

        Task<bool> ComplateAsync(StatuTypes types = StatuTypes.Unset, CancellationToken token = default);
    }
}