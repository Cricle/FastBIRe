using System.Data;

namespace FastBIRe.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class DbTypeAttribute : Attribute
    {
        public DbType DbType { get; set; } = DbType.String;

        public string? DataType { get; set; } 
    }
}
