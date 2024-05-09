namespace Diagnostics.Traces
{
    public class DelegatePhysicalPathProvider<TIdentity> : IPhysicalPathProvider<TIdentity>
        where TIdentity : IEquatable<TIdentity>
    {
        public DelegatePhysicalPathProvider(Func<TIdentity, string> getter)
        {
            Getter = getter ?? throw new ArgumentNullException(nameof(getter));
        }

        public Func<TIdentity,string> Getter { get; }

        public string GetPath(TIdentity identity)
        {
            return Getter(identity);
        }
    }
}
