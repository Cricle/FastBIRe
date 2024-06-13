namespace Diagnostics.Traces.Status
{
    public interface IStatusStorage : IStatusStorageStatistics,IEnumerable<IStatusScope>
    {
        string Name { get; }

        bool AddScope(IStatusScope scope);

        bool ComplatedScope(IStatusScope scope, StatusTypes types);

    }
}
