using FastBIRe.Project.Accesstor;

namespace FastBIRe.Project.WebSample
{
    public class ProjectDbServices : DbProjectFactoryBase<IProjectAccesstContext<string>, SchoolProject, string, ProjectCreateWithDbContextResult<SchoolProject, string>>
    {
        public ProjectDbServices(IProjectAccesstor<IProjectAccesstContext<string>, SchoolProject, string> projectAccesstor,
            IDataSchema<IProjectAccesstContext<string>> dataSchema,
            IStringToDbConnectionFactory stringToDbConnectionFactory,
            string connectionString)
            : base(projectAccesstor, dataSchema, stringToDbConnectionFactory, connectionString)
        {
        }

        public ProjectDbServices(IProjectAccesstor<IProjectAccesstContext<string>, SchoolProject, string> projectAccesstor,
            IEqualityComparer<string> equalityComparer,
            IDataSchema<IProjectAccesstContext<string>> dataSchema,
            IStringToDbConnectionFactory stringToDbConnectionFactory,
            string connectionString)
            : base(projectAccesstor, equalityComparer, dataSchema, stringToDbConnectionFactory, connectionString)
        {
        }

        protected override Task<ProjectCreateWithDbContextResult<SchoolProject, string>?> OnCreateResultHasFirstAsync(IProjectAccesstContext<string> input, SchoolProject project, bool isFirst, CancellationToken token = default)
        {
            return Task.FromResult<ProjectCreateWithDbContextResult<SchoolProject, string>?>(new ProjectCreateWithDbContextResult<SchoolProject, string>(project, isFirst, new MigrationService(CreateDbConnection(input))));
        }
        public Task<ITableFactory<ProjectCreateWithDbContextResult<SchoolProject, string>, SchoolProject, string>?> CreateTableFactoryAsync(IProjectAccesstContext<string> input, CancellationToken token = default)
        {
            return base.CreateTableFactoryAsync(input, TableIniter.Instance, token);
        }
    }
}