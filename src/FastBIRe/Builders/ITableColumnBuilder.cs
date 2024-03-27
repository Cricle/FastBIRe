using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.Builders
{
    public interface ITableColumnBuilder: ISqlTableBuilder
    {
        DatabaseTable Table { get; }

        DatabaseColumn Column { get; }

        ITableColumnBuilder Config(Action<DatabaseColumn> configuration);
    }
}
