namespace Diagnostics.Traces.Status
{
    public interface IStatusScope : IDisposable
    {
        string Name { get; }

        string Key { get; }

        bool IsComplated { get; }

        DateTime StartTime { get; }

        bool Set(string status);

        bool Log(string message);

        bool Complate(StatusTypes types = StatusTypes.Unset);

        Task<bool> ComplateAsync(StatusTypes types = StatusTypes.Unset, CancellationToken token = default);
    }
}