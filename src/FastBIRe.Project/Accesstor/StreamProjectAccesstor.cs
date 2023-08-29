using FastBIRe.Project.Models;

namespace FastBIRe.Project.Accesstor
{
    public class StreamProjectAccesstor : StreamProjectAccesstor<IProject<string>>
    {
        public StreamProjectAccesstor(IStreamProjectAdapter<IProjectAccesstContext<string>, string> adapter) : base(adapter)
        {
        }
    }
    public class StreamProjectAccesstor<TProject> : StreamProjectAccesstor<IProjectAccesstContext<string>, string>
           where TProject : IProject<string>
    {
        public StreamProjectAccesstor(IStreamProjectAdapter<IProjectAccesstContext<string>, string> adapter) : base(adapter)
        {
        }
    }
    public class StreamProjectAccesstor<TInput, TId> : ProjectAccesstorBase<TInput, TId>, IDisposable
           where TInput : IProjectAccesstContext<TId>
    {
        public StreamProjectAccesstor(IStreamProjectAdapter<TInput, TId> adapter)
        {
            Adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        }

        public IStreamProjectAdapter<TInput,TId> Adapter { get; }

        public override Task<IReadOnlyList<IProject<TId>>> AllProjectsAsync(TInput? input, CancellationToken cancellationToken = default)
        {
            return Adapter.AllProjectsAsync(input, cancellationToken);
        }

        public void Dispose()
        {
            Adapter.Dispose();
        }

        public override Task<bool> ProjectExistsAsync(TInput input, CancellationToken cancellationToken = default)
        {
            return Adapter.ProjectExistsAsync(input, cancellationToken);
        }

        protected override Task<int> OnCleanProjectAsync(TInput? input, CancellationToken cancellationToken = default)
        {
            return Adapter.CleanProjectAsync(input, cancellationToken);
        }

        protected override Task<bool> OnCreateProjectAsync(TInput input, IProject<TId> project, CancellationToken cancellationToken = default)
        {
            return Adapter.CreateProjectAsync(input, project, cancellationToken);
        }

        protected override Task<bool> OnDeleteProjectAsync(TInput input, CancellationToken cancellationToken = default)
        {
            return Adapter.DeleteProjectAsync(input, cancellationToken);
        }

        protected override Task<IProject<TId>?> OnGetProjectAsync(TInput input, CancellationToken cancellationToken = default)
        {
            return Adapter.GetProjectAsync(input, cancellationToken);
        }

        protected override Task<bool> OnUpdateProjectAsync(TInput input, IProject<TId> project, CancellationToken cancellationToken = default)
        {
            return Adapter.UpdateProjectAsync(input, project, cancellationToken);
        }
    }
}
