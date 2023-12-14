namespace FastBIRe.Annotations
{
    [AttributeUsage(AttributeTargets.Class| AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class GenerateModelAttribute : Attribute
    {
        public GenerateModelAttribute(bool isPublic = false)
        {
            IsPublic = isPublic;
        }

        public bool IsPublic { get; }
    }
}
