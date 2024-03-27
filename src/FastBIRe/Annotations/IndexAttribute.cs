using System.Data;

namespace FastBIRe.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class IndexAttribute : Attribute
    {
        public const int OneRowGroup = int.MinValue;

        public string? IndexName { get; set; }

        public int Order { get; set; } 

        public int IndexGroup { get; set; } = OneRowGroup;

        public bool IsDesc { get; set; }

        public bool IsOneRowGroup => IndexGroup == OneRowGroup;

        public override string ToString()
        {
            return $"{{Order:{Order}, IndexGroup:{(IsOneRowGroup ? "One" : IndexGroup)}, IndexName: {IndexName}}}";
        }
    }
}
