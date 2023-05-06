using FastBIRe.Project.Models;

namespace FastBIRe.Project.Accesstor
{
    public class CreatedProjectEventArgs<TInput, TId> : WithProjectEventArgs<TInput, TId>
           where TInput : IProjectAccesstContext<TId>
    {
        public CreatedProjectEventArgs(TInput input, IProject<TId>? project, bool succeed) : base(input, project)
        {
            Succeed = succeed;
        }

        public bool Succeed { get; }
    }
}
