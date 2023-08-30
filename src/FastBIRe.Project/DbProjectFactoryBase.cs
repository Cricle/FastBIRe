using DatabaseSchemaReader.DataSchema;
using FastBIRe.Project.Accesstor;
using FastBIRe.Project.Models;
using System.Data.Common;

namespace FastBIRe.Project
{
    public abstract class DbProjectFactoryBase<TInput, TProject, TId, TResult> : ProjectFactoryBase<TInput, TProject, TId, TResult>
        where TInput : IProjectAccesstContext<TId>
        where TId : notnull
        where TProject : IProject<TId>
        where TResult : ProjectCreateWithDbContextResult<TProject, TId>
    {
        protected DbProjectFactoryBase(IProjectAccesstor<TInput, TProject, TId> projectAccesstor, IDataSchema<TInput> dataSchema,
            IStringToDbConnectionFactory stringToDbConnectionFactory, string connectionString)
            : this(projectAccesstor, EqualityComparer<TId>.Default, dataSchema, stringToDbConnectionFactory, connectionString)
        {
        }

        protected DbProjectFactoryBase(IProjectAccesstor<TInput, TProject, TId> projectAccesstor,
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

        protected sealed override async Task<TResult?> OnCreateResultAsync(TInput input, TProject project, bool isFirst, CancellationToken token = default)
        {
            var database = DataSchema.GetDatabaseName(input);
            if (isFirst && NeetToInitDatabase(input, project))
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

        protected virtual bool NeetToInitDatabase(TInput input, TProject project)
        {
            return StringToDbConnectionFactory.SqlType != SqlType.SQLite;
        }

        protected abstract Task<TResult?> OnCreateResultHasFirstAsync(TInput input, TProject project, bool isFirst, CancellationToken token = default);


        public virtual async Task<ITableFactory<TResult, TProject, TId>?> CreateTableFactoryAsync(TInput input, ITableIniter tableIniter, CancellationToken token = default)
        {
            var result = await CreateDbContextAsync(input, token);
            if (result == null)
            {
                return null;
            }
            return CreateTableFactory(result, tableIniter);
        }
        public virtual ITableFactory<TResult, TProject, TId> CreateTableFactory(TResult result, ITableIniter tableIniter)
        {
            return new TableFactory<TResult, TProject, TId>(new MigrationService(result.Connection), tableIniter, result);
        }
    }

}
