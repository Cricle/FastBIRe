using FastBIRe.Project.Models;

namespace FastBIRe.Project.Accesstor
{
    public abstract class ProjectAccesstorBase<TInput, TId> : IProjectAccesstor<TInput, TId>
        where TInput : IProjectAccesstContext<TId>
    {
        public event EventHandler<WithProjectEventArgs<TInput, TId>>? OnGetProjected;
        public event EventHandler<ProjectAccessEventArgs<TInput, TId>>? OnDeletingProject;
        public event EventHandler<BoolProjectEventArgs<TInput, TId>>? OnDeletedProject;
        public event EventHandler<CreatingProjectEventArgs<TInput, TId>>? OnCreatingProject;
        public event EventHandler<CreatedProjectEventArgs<TInput, TId>>? OnCreatedProject;
        public event EventHandler<CleaningProjectEventArgs<TInput, TId>>? OnCleaningProject;
        public event EventHandler<CleanProjectEventArgs<TInput, TId>>? OnCleanProject;


        public async Task<int> CleanProjectAsync(TInput? input, CancellationToken cancellationToken = default)
        {
            OnCleaningProject?.Invoke(this, new CleaningProjectEventArgs<TInput, TId>(input));
            var res = await OnCleanProjectAsync(input, cancellationToken).ConfigureAwait(false);
            OnCleanProject?.Invoke(this, new CleanProjectEventArgs<TInput, TId>(input, res));
            return res;
        }

        protected abstract Task<int> OnCleanProjectAsync(TInput? input, CancellationToken cancellationToken = default);

        public async Task<bool> CreateProjectAsync(TInput input, IProject<TId> project, CancellationToken cancellationToken = default)
        {
            OnCreatingProject?.Invoke(this, new CreatingProjectEventArgs<TInput, TId>(input, project));
            var res = await OnCreateProjectAsync(input, project, cancellationToken).ConfigureAwait(false);
            OnCreatedProject?.Invoke(this, new CreatedProjectEventArgs<TInput, TId>(input, project, res));
            return res;
        }

        protected abstract Task<bool> OnCreateProjectAsync(TInput input, IProject<TId> project, CancellationToken cancellationToken = default);

        public async Task<bool> DeleteProjectAsync(TInput input, CancellationToken cancellationToken = default)
        {
            OnDeletingProject?.Invoke(this, new ProjectAccessEventArgs<TInput, TId>(input));
            var res = await OnDeleteProjectAsync(input, cancellationToken).ConfigureAwait(false);
            OnDeletedProject?.Invoke(this, new BoolProjectEventArgs<TInput, TId>(input, res));
            return res;
        }
        protected abstract Task<bool> OnDeleteProjectAsync(TInput input, CancellationToken cancellationToken = default);

        public async Task<IProject<TId>?> GetProjectAsync(TInput input, CancellationToken cancellationToken = default)
        {
            var res = await OnGetProjectAsync(input, cancellationToken).ConfigureAwait(false);
            OnGetProjected?.Invoke(this, new WithProjectEventArgs<TInput, TId>(input, res));
            return res;
        }

        protected abstract Task<IProject<TId>?> OnGetProjectAsync(TInput input, CancellationToken cancellationToken = default);

        public abstract Task<bool> ProjectExistsAsync(TInput input, CancellationToken cancellationToken = default);

        public abstract Task<IReadOnlyList<IProject<TId>>> AllProjectsAsync(TInput? input, CancellationToken cancellationToken = default);
    }
}
