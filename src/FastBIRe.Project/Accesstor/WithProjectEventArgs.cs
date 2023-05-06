using FastBIRe.Project.Models;

namespace FastBIRe.Project.Accesstor
{
    public class WithProjectEventArgs<TInput, TId> : ProjectAccessEventArgs<TInput, TId>
           where TInput : IProjectAccesstContext<TId>
    {
        public WithProjectEventArgs(TInput input, IProject<TId>? project) : base(input)
        {
            Project = project;
        }

        public IProject<TId>? Project { get; }
    }
}
