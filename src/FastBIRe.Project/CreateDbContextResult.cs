using FastBIRe.Project.Models;
using System.Data.Common;

namespace FastBIRe.Project
{
    public record class ProjectCreateContextResult<TId>(IProject<TId>? Project, bool IsFirst);
    public record class ProjectCreateWithDbContextResult<TId>(IProject<TId>? Project, bool IsFirst,DbConnection Connection):
        ProjectCreateContextResult<TId>(Project,IsFirst);

}
