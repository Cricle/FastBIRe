namespace FastBIRe.Project.Models
{
    public interface IPhysicalProject<TId> : IProject<TId>
    {
        string? GetFilePath();
    }
}
