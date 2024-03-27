namespace FastBIRe
{
    public class MigrationScriptsResultBase
    {
        public MigrationScriptsResultBase(IList<string> scripts)
        {
            Scripts = scripts;
        }

        public IList<string> Scripts { get; }

        public string Script => ToString();

        public override string ToString()
        {
            if (Scripts.Count == 0)
            {
                return string.Empty;
            }
            return string.Join(Environment.NewLine, Scripts);
        }

    }
    public class MigrationScriptResult : MigrationScriptsResultBase
    {
        public MigrationScriptResult(string tableName, IList<string> scripts)
            : base(scripts)
        {
            TableName = tableName;
        }

        public string TableName { get; }
    }
    public class MigrationScriptsResult : MigrationScriptsResultBase
    {
        public MigrationScriptsResult(IList<string> tableNames, IList<string> scripts)
            : base(scripts)
        {
            TableNames = tableNames;
        }

        public IList<string> TableNames { get; }
    }
}
