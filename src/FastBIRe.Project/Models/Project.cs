namespace FastBIRe.Project.Models
{
    public record Project<TId>(TId Id, string Name,string Version, DateTime CreateTime) : IProject<TId>;
}
