using DatabaseSchemaReader.DataSchema;
using FastBIRe.Builders;

namespace FastBIRe
{
    public interface ITableProvider : IEnumerable<DatabaseTable>,ISqlTableBuilder
    {
        bool IsDynamic { get; }

        bool HasTable(string name);

        DatabaseTable? GetTable(string name);
    }
}
