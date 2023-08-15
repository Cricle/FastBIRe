using FastBIRe.Project.Models;
using System.IO.Compression;
#if !NETSTANDARD2_0
using System.Text.Json;
#endif

namespace FastBIRe.Project.Accesstor
{
#if !NETSTANDARD2_0
    public class JsonZipStreamProjectAdapter : JsonZipStreamProjectAdapter<Project<string>>
    {
        public JsonZipStreamProjectAdapter(ZipArchive archive) : base(archive)
        {
        }
    }
    public class JsonZipStreamProjectAdapter<TProject> : JsonZipStreamProjectAdapter<TProject, IProjectAccesstContext<string>, string>
           where TProject : IProject<string>
    {
        public JsonZipStreamProjectAdapter(ZipArchive archive) : base(archive)
        {
        }
        public override string? GetEntryName(IProjectAccesstContext<string> input)
        {
            return input.Id;
        }
    }
    public class JsonZipStreamProjectAdapter<TProject,TInput, TId> : ZipStreamProjectAdapterBase<TInput, TId>
        where TInput : IProjectAccesstContext<TId>
        where TProject : IProject<TId>
    {
        public JsonZipStreamProjectAdapter(ZipArchive archive) : base(archive)
        {
        }

        public JsonSerializerOptions? Options { get; set; }

        public override IProject<TId>? ConvertToProject(ZipArchiveEntry entry)
        {
            using (var fs = entry.Open())
            {
                return JsonSerializer.Deserialize<TProject>(fs, Options);
            }
        }

        public override async Task WriteProjectToFileAsync(ZipArchiveEntry entry, IProject<TId> project, CancellationToken cancellationToken = default)
        {
            using (var fs = entry.Open())
            {
                fs.SetLength(0);
                await JsonSerializer.SerializeAsync(fs, project, Options, cancellationToken);
            }
        }
    }
#endif
    public abstract class ZipStreamProjectAdapterBase<TInput, TId> : IStreamProjectAdapter<TInput, TId>
        where TInput : IProjectAccesstContext<TId>
    {
        protected ZipStreamProjectAdapterBase(ZipArchive archive)
        {
            Archive = archive ?? throw new ArgumentNullException(nameof(archive));
        }

        public ZipArchive Archive { get; }

        public abstract IProject<TId>? ConvertToProject(ZipArchiveEntry entry);

        public abstract Task WriteProjectToFileAsync(ZipArchiveEntry entry, IProject<TId> project, CancellationToken cancellationToken = default);

        public virtual bool IsProject(ZipArchiveEntry entry)
        {
            return true;
        }
        public virtual string? GetEntryName(TInput input)
        {
            return input?.ToString();
        }
        public virtual IEnumerable<ZipArchiveEntry> EnumerableProjectEntity(string? keyword)
        {
            return Archive.Entries.Where(x => string.IsNullOrEmpty(keyword) || x.Name.Contains(keyword));
        }
        public Task<IReadOnlyList<IProject<TId>>> AllProjectsAsync(TInput? input, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var res = EnumerableProjectEntity(input?.ToString())
                .Where(IsProject)
                .Select(ConvertToProject)
                .Where(x => x != null)
                .ToList();
            return Task.FromResult<IReadOnlyList<IProject<TId>>>(res!);
        }

        public Task<int> CleanProjectAsync(TInput? input, CancellationToken cancellationToken = default)
        {
            var rms = EnumerableProjectEntity(input?.ToString()).Where(IsProject);
            var res = 0;
            foreach (var item in rms)
            {
                item.Delete();
                res++;
            }
            return Task.FromResult(res);
        }

        public async Task<bool> CreateProjectAsync(TInput input, IProject<TId> project, CancellationToken cancellationToken = default)
        {
            var name = GetEntryName(input);
            var zipEntity=Archive.GetEntry(name);
            if (zipEntity == null)
            {
                zipEntity=Archive.CreateEntry(name);
            }
            await WriteProjectToFileAsync(zipEntity, project, cancellationToken).ConfigureAwait(false);
            return true;
        }

        public Task<bool> DeleteProjectAsync(TInput input, CancellationToken cancellationToken = default)
        {
            var name = GetEntryName(input);
            var zipEntity = Archive.GetEntry(name);
            if (zipEntity==null)
            {
                return Task.FromResult(false);
            }
            zipEntity.Delete();
            return Task.FromResult(true);
        }

        public void Dispose()
        {
            Archive.Dispose();
        }

        public Task<IProject<TId>?> GetProjectAsync(TInput input, CancellationToken cancellationToken = default)
        {
            var name = GetEntryName(input);
            var zipEntity = Archive.GetEntry(name);
            if (zipEntity==null)
            {
                return Task.FromResult<IProject<TId>?>(null);
            }
            return Task.FromResult(ConvertToProject(zipEntity));
        }

        public Task<bool> ProjectExistsAsync(TInput input, CancellationToken cancellationToken = default)
        {
            var name = GetEntryName(input);
            return Task.FromResult(Archive.Entries.Any(x => x.Name == name));
        }
    }
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
