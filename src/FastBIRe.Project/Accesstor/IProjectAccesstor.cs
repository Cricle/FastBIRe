using FastBIRe.Project.Models;

namespace FastBIRe.Project.Accesstor
{
    public interface IProjectAccesstor<TInput, TProject, TId>
        where TProject : IProject<TId>
        where TInput : IProjectAccesstContext<TId>
    {
        event EventHandler<WithProjectEventArgs<TInput, TId>>? OnGetProjected;

        event EventHandler<ProjectAccessEventArgs<TInput, TId>>? OnDeletingProject;
        event EventHandler<BoolProjectEventArgs<TInput, TId>>? OnDeletedProject;

        event EventHandler<CreatingProjectEventArgs<TInput, TId>>? OnCreatingProject;
        event EventHandler<CreatedProjectEventArgs<TInput, TId>>? OnCreatedProject;

        event EventHandler<UpdatingProjectEventArgs<TInput, TId>>? OnUpdatingProject;
        event EventHandler<UpdatedProjectEventArgs<TInput, TId>>? OnUpdatedProject;

        event EventHandler<CleaningProjectEventArgs<TInput, TId>>? OnCleaningProject;
        event EventHandler<CleanProjectEventArgs<TInput, TId>>? OnCleanProject;

        Task<TProject?> GetProjectAsync(TInput input, CancellationToken cancellationToken = default);

        Task<bool> DeleteProjectAsync(TInput input, CancellationToken cancellationToken = default);

        Task<bool> CreateProjectAsync(TInput input, TProject project, CancellationToken cancellationToken = default);

        Task<bool> UpdateProjectAsync(TInput input, TProject project, CancellationToken cancellationToken = default);

        Task<bool> ProjectExistsAsync(TInput input, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<TProject>> AllProjectsAsync(TInput? input, CancellationToken cancellationToken = default);

        Task<int> CleanProjectAsync(TInput? input, CancellationToken cancellationToken = default);
    }
}
