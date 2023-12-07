using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.Builders
{
    public interface ITablesProviderBuilder: ISqlTableBuilder
    {
        ITableProvider Build();
        
        ITableBuilder GetTableBuilder(string name);
    }
}
