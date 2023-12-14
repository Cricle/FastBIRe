using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;

namespace FastBIRe.Builders
{
    public static class TableConfigerGetExtensions
    {
        public static string GetCreateTableScript(this ITableConfiger tableConfiger,string tableName,SqlType sqlType)
        {
            var table = new DatabaseTable { Name = tableName };
            var builder = new TableBuilder(table, sqlType);
            tableConfiger.Config(builder);
            return new DdlGeneratorFactory(sqlType).TableGenerator(table).Write();
        }
    }
}
