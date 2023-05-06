namespace FastBIRe.Project.Accesstor
{
    public class CleaningProjectEventArgs<TInput, TId> : ProjectAccessEventArgs<TInput, TId>
           where TInput : IProjectAccesstContext<TId>
    {
        public CleaningProjectEventArgs(TInput? input) : base(input)
        {
        }
    }
}
