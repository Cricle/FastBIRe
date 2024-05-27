namespace Diagnostics.Traces.Status
{
    public interface IStatusScope : IAsyncDisposable,IDisposable
    {
        string Name { get; }

        string Key { get; }

        bool IsComplated { get; }

        bool Set(string status);

        Task<bool> SetAsync(string status, CancellationToken token = default);

        bool Log(string message);

        Task<bool> LogAsync(string message, CancellationToken token = default);

        bool Complate(StatusTypes types = StatusTypes.Unset);

        Task<bool> ComplateAsync(StatusTypes types = StatusTypes.Unset, CancellationToken token = default);
    }
}