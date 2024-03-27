using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.Builders
{
    public interface ISqlTableBuilder
    {
        SqlType SqlType { get; }
    }
}
