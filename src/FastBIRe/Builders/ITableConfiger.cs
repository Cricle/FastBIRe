using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;

namespace FastBIRe.Builders
{
    public interface ITableConfiger
    {
        void Config(ITableBuilder builder);
    }
}
