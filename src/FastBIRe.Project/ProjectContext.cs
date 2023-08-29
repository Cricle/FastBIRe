using FastBIRe.Project.Models;

namespace FastBIRe.Project
{
    public class ProjectContext<TId>
    {
        public ProjectContext(IProject<TId> current)
        {
            Current = current;
        }

        public IProject<TId> Current { get; }
    }
}
