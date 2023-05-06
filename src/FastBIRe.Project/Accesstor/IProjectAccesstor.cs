using FastBIRe.Project.Models;

namespace FastBIRe.Project.Accesstor
{
    public interface IProjectAccesstor<TInput,TId>
        where TInput : IProjectAccesstContext<TId>
    {
        Task<IProject<TId>?> GetProjectAsync(TInput input, CancellationToken cancellationToken = default);

        Task<bool> DeleteProjectAsync(TInput input, CancellationToken cancellationToken = default);
        
        Task<bool> CreateProjectAsync(TInput input, IProject<TId> project, CancellationToken cancellationToken = default);

        Task<bool> ProjectExistsAsync(TInput input, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<IProject<TId>>> AllProjectsAsync(TInput? input, CancellationToken cancellationToken = default);

        Task<int> CleanProjectAsync(TInput? input,CancellationToken cancellationToken = default);
    }
}
