using FastBIRe.Project.Models;

namespace FastBIRe.Project.Accesstor
{
    public class CreatingProjectEventArgs<TInput, TId> : WithProjectEventArgs<TInput, TId>
           where TInput : IProjectAccesstContext<TId>
    {
        public CreatingProjectEventArgs(TInput input, IProject<TId>? project) : base(input, project)
        {
        }
    }
}
