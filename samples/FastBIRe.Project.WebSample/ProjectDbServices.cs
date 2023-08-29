using FastBIRe.Project.Accesstor;
using FastBIRe.Project.Models;

namespace FastBIRe.Project.WebSample
{
    public class ProjectDbServices : DbProjectFactoryBase<IProjectAccesstContext<string>, string, ProjectCreateWithDbContextResult<string>>
    {
        public ProjectDbServices(IProjectAccesstor<IProjectAccesstContext<string>, string> projectAccesstor,
            IDataSchema<IProjectAccesstContext<string>> dataSchema,
            IStringToDbConnectionFactory stringToDbConnectionFactory,
            string connectionString)
            : base(projectAccesstor, dataSchema, stringToDbConnectionFactory, connectionString)
        {
        }

        public ProjectDbServices(IProjectAccesstor<IProjectAccesstContext<string>, string> projectAccesstor,
            IEqualityComparer<string> equalityComparer,
            IDataSchema<IProjectAccesstContext<string>> dataSchema,
            IStringToDbConnectionFactory stringToDbConnectionFactory,
            string connectionString)
            : base(projectAccesstor, equalityComparer, dataSchema, stringToDbConnectionFactory, connectionString)
        {
        }

        protected override Task<ProjectCreateWithDbContextResult<string>?> OnCreateResultHasFirstAsync(IProjectAccesstContext<string> input, IProject<string> project, bool isFirst, CancellationToken token = default)
        {
            return Task.FromResult<ProjectCreateWithDbContextResult<string>?>(new ProjectCreateWithDbContextResult<string>(project, isFirst, CreateDbConnection(input)));
        }
        public Task<ITableFactory<ProjectCreateWithDbContextResult<string>, string>?> CreateTableFactoryAsync(IProjectAccesstContext<string> input,CancellationToken token = default)
        {
            return base.CreateTableFactoryAsync(input, TableIniter.Instance, token);
        }
    }
}