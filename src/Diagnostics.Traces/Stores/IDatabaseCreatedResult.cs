namespace Diagnostics.Traces.Stores
{
    public interface IDatabaseCreatedResult : IDisposable
    {
        object Root { get; }

        string? FilePath { get; }

        string Key { get; }
    }
}
