using DatabaseSchemaReader.DataSchema;
using FastBIRe.Project.Accesstor;
using FastBIRe.Project.Models;
using System.Data.Common;

namespace FastBIRe.Project
{
    public abstract class DbProjectFactoryBase<TInput, TId, TResult> : ProjectFactoryBase<TInput, TId, TResult>
        where TInput : IProjectAccesstContext<TId>
        where TId : notnull
        where TResult : ProjectCreateWithDbContextResult<TId>
    {
        protected DbProjectFactoryBase(IProjectAccesstor<TInput, TId> projectAccesstor, IDataSchema<TInput> dataSchema,
            IStringToDbConnectionFactory stringToDbConnectionFactory, string connectionString)
            : this(projectAccesstor, EqualityComparer<TId>.Default, dataSchema, stringToDbConnectionFactory, connectionString)
        {
        }

        protected DbProjectFactoryBase(IProjectAccesstor<TInput, TId> projectAccesstor,
            IEqualityComparer<TId> equalityComparer,
            IDataSchema<TInput> dataSchema,
            IStringToDbConnectionFactory stringToDbConnectionFactory,
            string connectionString)
            : base(projectAccesstor, equalityComparer, dataSchema)
        {
            StringToDbConnectionFactory = stringToDbConnectionFactory;
            ConnectionString = connectionString;
        }

        public IStringToDbConnectionFactory StringToDbConnectionFactory { get; }

        public string ConnectionString { get; }

        public Action<string>? Logger { get; set; }

        protected virtual DbConnection CreateDbConnection(TInput input)
        {
            return StringToDbConnectionFactory.CreateDbConnection(ConnectionString,
                DataSchema.GetDatabaseName(input));
        }

        protected sealed override async Task<TResult?> OnCreateResultAsync(TInput input, IProject<TId> project, bool isFirst, CancellationToken token = default)
        {
            var database = DataSchema.GetDatabaseName(input);
            if (isFirst&& NeetToInitDatabase(input,project))
            {
                using (var db = StringToDbConnectionFactory.CreateDbConnection(ConnectionString))
                using (var migSer = new MigrationService(db))
                {
                    migSer.Logger = Logger;
                    await migSer.EnsureDatabaseCreatedAsync(database, token);
                }
            }
            return await OnCreateResultHasFirstAsync(input, project, isFirst, token);
        }

        protected virtual bool NeetToInitDatabase(TInput input, IProject<TId> project)
        {
            return StringToDbConnectionFactory.SqlType != SqlType.SQLite;
        }

        protected abstract Task<TResult?> OnCreateResultHasFirstAsync(TInput input, IProject<TId> project, bool isFirst, CancellationToken token = default);


        public virtual async Task<ITableFactory<TResult, TId>?> CreateTableFactoryAsync(TInput input, ITableIniter tableIniter, CancellationToken token = default)
        {
            var result = await CreateDbContextAsync(input, token);
            if (result == null)
            {
                return null;
            }
            return new TableFactory<TResult, TId>(new MigrationService(result.Connection), tableIniter, result);
        }
    }

}
