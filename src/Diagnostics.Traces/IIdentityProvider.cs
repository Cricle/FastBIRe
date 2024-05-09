namespace Diagnostics.Traces
{
    public interface IIdentityProvider<TIdentity, TInput>
        where TIdentity : IEquatable<TIdentity>
    {
        bool HasIdentity(TInput identity);

        TIdentity GetIdentity(TInput input);
    }
}
