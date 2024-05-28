namespace Diagnostics.Traces
{
    public class DelegateIdentityProvider<TIdentity, TInput> : IIdentityProvider<TIdentity, TInput>
        where TIdentity : IEquatable<TIdentity>
    {
        public DelegateIdentityProvider(Func<TInput, GetIdentityResult<TIdentity>> getter)
        {
            Getter = getter;
        }

        public Func<TInput, GetIdentityResult<TIdentity>> Getter { get; }

        public GetIdentityResult<TIdentity> GetIdentity(TInput input)
        {
            return Getter(input);
        }
    }
}
