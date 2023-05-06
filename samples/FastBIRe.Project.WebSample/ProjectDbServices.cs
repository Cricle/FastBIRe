using FastBIRe.Project.Accesstor;
using FastBIRe.Project.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace FastBIRe.Project.WebSample
{
    public readonly struct CreateDbContextResult
    {
        public readonly SchoolDbContext? DbContext;

        public readonly IProject<string> Project;

        public CreateDbContextResult(SchoolDbContext? dbContext, IProject<string> project)
        {
            DbContext = dbContext;
            Project = project;
        }
    }
    public class ProjectDbServices
    {
        public ProjectDbServices(IProjectAccesstor<IProjectAccesstContext<string>, string> projectAccesstor)
            :this(projectAccesstor,new DbContextOptionsBuilder().Options)
        {

        }
        public ProjectDbServices(IProjectAccesstor<IProjectAccesstContext<string>, string> projectAccesstor, DbContextOptions baseOption)
        {
            ProjectAccesstor = projectAccesstor;
            BaseOption = baseOption;

            projectFirst = new ConcurrentDictionary<string, bool>();
        }
        private readonly ConcurrentDictionary<string, bool> projectFirst;

        public IProjectAccesstor<IProjectAccesstContext<string>, string> ProjectAccesstor { get; }

        public DbContextOptions BaseOption { get; }

        public async Task<CreateDbContextResult> CreateDbContextAsync(string id, CancellationToken token = default)
        {
            var project = await ProjectAccesstor.GetProjectAsync(new ProjectAccesstContext<string>(id), token);
            if (project != null)
            {
                var builder = new DbContextOptionsBuilder(BaseOption);
                builder.UseSqlite($"Data source=dbs/{project.Id}");
                var ctx = new SchoolDbContext(builder.Options);
                if (projectFirst.TryAdd(id, false))
                {
                    await ctx.Database.EnsureCreatedAsync();
                    await ctx.Database.MigrateAsync();
                }
                return new CreateDbContextResult(ctx, project);
            }
            return default;
        }
    }
}