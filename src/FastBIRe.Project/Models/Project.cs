namespace FastBIRe.Project.Models
{
    public record Project<TId>(TId Id, string Name, Version Version, DateTime CreateTime) : IProject<TId>;
}
