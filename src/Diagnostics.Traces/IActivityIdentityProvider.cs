using System.Diagnostics;

namespace Diagnostics.Traces
{
    public interface IActivityIdentityProvider<TIdentity> : IIdentityProvider<TIdentity, Activity>
        where TIdentity : IEquatable<TIdentity>
    {

    }
}
