using OpenTelemetry.Metrics;

namespace Diagnostics.Traces
{
    public interface IMiniWriteSerializer<TIdentity> : IIdentityProvider<TIdentity, Metric>
        where TIdentity : IEquatable<TIdentity>
    {

    }
}
