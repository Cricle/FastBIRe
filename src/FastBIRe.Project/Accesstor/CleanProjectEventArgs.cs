namespace FastBIRe.Project.Accesstor
{
    public class CleanProjectEventArgs<TInput, TId> : ProjectAccessEventArgs<TInput, TId>
           where TInput : IProjectAccesstContext<TId>
    {
        public CleanProjectEventArgs(TInput? input, int affect)
            : base(input)
        {
            Affect = affect;
        }

        public int Affect { get; }
    }
}
