namespace FastBIRe.Project.Accesstor
{
    public class BoolProjectEventArgs<TInput,TId> : ProjectAccessEventArgs<TInput,TId>
           where TInput : IProjectAccesstContext<TId>
    {
        public BoolProjectEventArgs(TInput input,bool succeed) : base(input)
        {
            Succeed = succeed;
        }

        public bool Succeed { get; }
    }
}
