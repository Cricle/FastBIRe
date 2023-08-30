using FastBIRe.Project.Models;
using System.Data.Common;

namespace FastBIRe.Project
{
    public record class ProjectCreateContextResult<TProject, TId>(TProject? Project, bool IsFirst)
        where TProject : IProject<TId>;
    public record class ProjectCreateWithDbContextResult<TProject, TId>(TProject? Project, bool IsFirst, MigrationService MigrationService) :
        ProjectCreateContextResult<TProject, TId>(Project, IsFirst), IDisposable
        where TProject : IProject<TId>
    {
        public DbConnection Connection => MigrationService.Connection;

        public void Dispose()
        {
            MigrationService?.Dispose();
        }
    }

}
