namespace Diagnostics.Traces.Status
{
    public interface IStatusStorageManager : ICollection<IStatusStorage>, IEnumerable<IStatusStorage>
    {
        bool TryGetValue(string name,out IStatusStorage storage);
    }
}
