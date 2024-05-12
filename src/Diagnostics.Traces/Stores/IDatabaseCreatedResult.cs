namespace Diagnostics.Traces.Stores
{
    public interface IDatabaseCreatedResult : IDisposable
    {
        public object Root { get; }

        public string? FilePath { get; }
    }
}
