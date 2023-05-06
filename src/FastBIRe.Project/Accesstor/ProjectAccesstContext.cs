namespace FastBIRe.Project.Accesstor
{
    public class ProjectAccesstContext<TId> : IProjectAccesstContext<TId>
    {
        public ProjectAccesstContext(TId id)
        {
            Id = id;
        }

        public TId Id { get; }
    }
}
