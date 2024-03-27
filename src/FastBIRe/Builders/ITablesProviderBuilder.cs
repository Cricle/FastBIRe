using DatabaseSchemaReader.DataSchema;
using System.Data.Common;

namespace FastBIRe.Builders
{
    public interface ITablesProviderBuilder: ISqlTableBuilder
    {
        ITableProvider Build();
        
        ITableBuilder GetTableBuilder(string name);
    }
    public static class TablesProviderBuilderBuildExtensions
    {
        public static IFastBIReContext BuildContext(this ITablesProviderBuilder builder, IDbScriptExecuter executer)
        {
            return new FastBIReContext(executer, builder.Build());
        }
        public static IFastBIReContext BuildContext(this ITablesProviderBuilder builder,DbConnection dbConnection)
        {
            return FastBIReContext.FromDbConnection(dbConnection, builder.Build());
        }
    }
}
