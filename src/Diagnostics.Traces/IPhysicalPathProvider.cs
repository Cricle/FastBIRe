namespace Diagnostics.Traces
{
    public interface IPhysicalPathProvider<TIdentity>
        where TIdentity : IEquatable<TIdentity>
    {
        string GetPath(TIdentity identity);
    }
}
