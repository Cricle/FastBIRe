namespace FastBIRe.Project
{
    public class DefaultDataSchema<TInput> : IDataSchema<TInput>
    {
        public DefaultDataSchema(string databaseNameFormat, string tableNameFormat)
        {
            DatabaseNameFormat = databaseNameFormat ?? throw new ArgumentNullException(nameof(databaseNameFormat));
            TableNameFormat = tableNameFormat ?? throw new ArgumentNullException(nameof(tableNameFormat));
        }

        public string DatabaseNameFormat { get; }

        public string TableNameFormat { get; }

        public string GetDatabaseName(TInput input)
        {
            return string.Format(DatabaseNameFormat, input);
        }

        public string GetTableName(TInput input)
        {
            return string.Format(TableNameFormat, input);
        }
    }

}
