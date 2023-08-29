using FastBIRe.Project.Models;

namespace FastBIRe.Project.Accesstor
{
    public class StreamProjectAccesstor : StreamProjectAccesstor<Project<string>>
    {
        public StreamProjectAccesstor(IStreamProjectAdapter<IProjectAccesstContext<string>, Project<string>, string> adapter) : base(adapter)
        {
        }
    }
    public class StreamProjectAccesstor<TProject> : StreamProjectAccesstor<IProjectAccesstContext<string>, TProject, string>
           where TProject : IProject<string>
    {
        public StreamProjectAccesstor(IStreamProjectAdapter<IProjectAccesstContext<string>, TProject, string> adapter) : base(adapter)
        {
        }
    }
    public class StreamProjectAccesstor<TInput, TProject, TId> : ProjectAccesstorBase<TInput, TProject, TId>, IDisposable
        where TProject : IProject<TId>
        where TInput : IProjectAccesstContext<TId>
    {
        public StreamProjectAccesstor(IStreamProjectAdapter<TInput, TProject, TId> adapter)
        {
            Adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        }

        public IStreamProjectAdapter<TInput, TProject, TId> Adapter { get; }

        public override Task<IReadOnlyList<TProject>> AllProjectsAsync(TInput? input, CancellationToken cancellationToken = default)
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

        protected override Task<bool> OnCreateProjectAsync(TInput input, TProject project, CancellationToken cancellationToken = default)
        {
            return Adapter.CreateProjectAsync(input, project, cancellationToken);
        }

        protected override Task<bool> OnDeleteProjectAsync(TInput input, CancellationToken cancellationToken = default)
        {
            return Adapter.DeleteProjectAsync(input, cancellationToken);
        }

        protected override Task<TProject?> OnGetProjectAsync(TInput input, CancellationToken cancellationToken = default)
        {
            return Adapter.GetProjectAsync(input, cancellationToken);
        }

        protected override Task<bool> OnUpdateProjectAsync(TInput input, TProject project, CancellationToken cancellationToken = default)
        {
            return Adapter.UpdateProjectAsync(input, project, cancellationToken);
        }
    }
}
