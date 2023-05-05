namespace FastBIRe.Project.Models
{
    public interface IProject<TId>
    {
        TId Id { get; }

        string Name { get; }

        string Version { get; }

        DateTime CreateTime { get; }
    }
}
