namespace Diagnostics.Traces
{
    public class DelegateIdentityProvider<TIdentity, TInput> : IIdentityProvider<TIdentity, TInput>
        where TIdentity : IEquatable<TIdentity>
    {
        public DelegateIdentityProvider(Func<TInput, bool> check,Func<TInput, TIdentity> getter)
        {
            Check = check ?? throw new ArgumentNullException(nameof(check));
            Getter = getter ?? throw new ArgumentNullException(nameof(getter));
        }

        public Func<TInput, TIdentity> Getter { get; }

        public Func<TInput, bool> Check { get; }

        public TIdentity GetIdentity(TInput input)
        {
            return Getter(input);
        }

        public bool HasIdentity(TInput identity)
        {
            return Check(identity);
        }
    }
}
