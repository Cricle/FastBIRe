namespace FastBIRe.Annotations
{
    [AttributeUsage(AttributeTargets.Method,AllowMultiple =false,Inherited = false)]
    public sealed class CreateAfterMethodAttribute:Attribute
    {
        public int Order { get; set; }
    }
}
