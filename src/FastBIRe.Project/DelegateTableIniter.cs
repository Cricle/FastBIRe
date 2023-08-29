namespace FastBIRe.Project
{
    public class DelegateTableIniter : ITableIniter
    {
        public DelegateTableIniter(Func<MigrationService, string, string> getInitScriptFun, Func<SourceTableColumnBuilder, IReadOnlyList<TableColumnDefine>> getInitTableColumnDefinesFun)
        {
            GetInitScriptFun = getInitScriptFun ?? throw new ArgumentNullException(nameof(getInitScriptFun));
            GetInitTableColumnDefinesFun = getInitTableColumnDefinesFun ?? throw new ArgumentNullException(nameof(getInitTableColumnDefinesFun));
        }

        public Func<MigrationService, string, string> GetInitScriptFun { get; }
        public Func<SourceTableColumnBuilder, IReadOnlyList<TableColumnDefine>> GetInitTableColumnDefinesFun { get; }

        public string GetInitScript(MigrationService migrationService, string tableName)
        {
            return GetInitScriptFun(migrationService,tableName);
        }

        public IReadOnlyList<TableColumnDefine> GetInitTableColumnDefines(SourceTableColumnBuilder builder)
        {
            return GetInitTableColumnDefinesFun(builder);
        }
    }

}
