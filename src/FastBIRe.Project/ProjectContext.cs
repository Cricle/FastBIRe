using FastBIRe.Project.Models;

namespace FastBIRe.Project
{
    public interface IProjectContext<TId>
    {
        IProject<TId> Current { get; }
    }
    public class ProjectContext<TId>
    {
        public ProjectContext(IProject<TId> current)
        {
            Current = current;
        }

        public IProject<TId> Current { get; }
    }
}
