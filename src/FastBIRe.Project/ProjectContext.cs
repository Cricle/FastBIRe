using FastBIRe.Project.Accesstor;
using FastBIRe.Project.Models;
using System.Collections.Concurrent;

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
