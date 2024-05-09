using OpenTelemetry.Metrics;

namespace Diagnostics.Traces
{
    public interface IMetricIdentityProvider<TIdentity> : IIdentityProvider<TIdentity, Metric>
        where TIdentity : IEquatable<TIdentity>
    {

    }
}
