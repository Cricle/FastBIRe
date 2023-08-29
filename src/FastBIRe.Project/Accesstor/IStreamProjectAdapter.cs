using FastBIRe.Project.Models;

namespace FastBIRe.Project.Accesstor
{
    public interface IStreamProjectAdapter<TInput,TProject, TId> : IDisposable
        where TProject:IProject<TId>
        where TInput : IProjectAccesstContext<TId>
    {
        Task<IReadOnlyList<TProject>> AllProjectsAsync(TInput? input, CancellationToken cancellationToken = default);

        Task<bool> ProjectExistsAsync(TInput input, CancellationToken cancellationToken = default);

        Task<int> CleanProjectAsync(TInput? input, CancellationToken cancellationToken = default);

        Task<bool> CreateProjectAsync(TInput input, TProject project, CancellationToken cancellationToken = default);

        Task<bool> UpdateProjectAsync(TInput input, TProject project, CancellationToken cancellationToken = default);

        Task<bool> DeleteProjectAsync(TInput input, CancellationToken cancellationToken = default);

        Task<TProject?> GetProjectAsync(TInput input, CancellationToken cancellationToken = default);
    }
}
