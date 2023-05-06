namespace FastBIRe.Project.Accesstor
{
    public class ProjectAccessEventArgs<TInput, TId> : EventArgs
          where TInput : IProjectAccesstContext<TId>
    {
        public ProjectAccessEventArgs(TInput? input)
        {
            Input = input;
        }

        public TInput? Input { get; set; }
    }
}
