using FastBIRe.Project.Models;

namespace FastBIRe.Project
{
    public interface IProjectContext<TId>
    {
        IProject<TId> Current { get; }
    }
}
