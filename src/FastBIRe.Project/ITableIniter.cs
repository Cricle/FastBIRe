namespace FastBIRe.Project
{
    public interface ITableIniter
    {
        IReadOnlyList<TableColumnDefine> GetInitTableColumnDefines(SourceTableColumnBuilder builder);

        string GetInitScript(MigrationService migrationService, string tableName);
    }
    public static class TableIniterExtensions
    {
        public static List<TableColumnDefine> WithColumns(this ITableIniter initer, SourceTableColumnBuilder builder, IEnumerable<TableColumnDefine> columns)
        {
            return initer.GetInitTableColumnDefines(builder).Concat(columns).ToList();
        }
    }

}
