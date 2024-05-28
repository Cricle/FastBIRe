using OpenTelemetry.Logs;

namespace Diagnostics.Traces
{
    public interface ILogIdentityProvider<TIdentity> : IIdentityProvider<TIdentity, LogRecord>
        where TIdentity : IEquatable<TIdentity>
    {

    }
}
