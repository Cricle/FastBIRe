namespace FastBIRe.Project
{
    public interface IDataSchema<TInput>
    {
        string GetDatabaseName(TInput input);

        string GetTableName(TInput input);
    }

    public class DelegateDataSchema<TInput> : IDataSchema<TInput>
    {
        public DelegateDataSchema(Func<TInput, string> databaseName, Func<TInput, string> tableName)
        {
            DatabaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
            TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }

        public Func<TInput, string> DatabaseName { get; }

        public Func<TInput, string> TableName { get; }

        public string GetDatabaseName(TInput input)
        {
            return DatabaseName(input);
        }

        public string GetTableName(TInput input)
        {
            return TableName(input);
        }
    }

}
