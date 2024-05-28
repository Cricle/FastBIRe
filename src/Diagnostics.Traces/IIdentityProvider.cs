namespace Diagnostics.Traces
{
    public interface IIdentityProvider<TIdentity, TInput>
        where TIdentity : IEquatable<TIdentity>
    {
        GetIdentityResult<TIdentity> GetIdentity(TInput input);
    }
    public readonly record struct GetIdentityResult<TIdentity>
    {
        public static readonly GetIdentityResult<TIdentity> Fail = new GetIdentityResult<TIdentity>(default, false);

        internal GetIdentityResult(TIdentity? identity, bool succeed)
        {
            Identity = identity;
            Succeed = succeed;
        }

        public GetIdentityResult(TIdentity? identity)
        {
            Identity = identity;
            Succeed = true;
        }

        public TIdentity? Identity { get; }

        public bool Succeed { get; }
    }
}
