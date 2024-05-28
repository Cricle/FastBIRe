namespace FastBIRe.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class AutoNumberAttribute : Attribute
    {
        public bool IdentityByDefault { get; set; } = true;

        public long IdentitySeed { get; set; } = 1;

        public long IdentityIncrement { get; set; } = 1;
    }
}
