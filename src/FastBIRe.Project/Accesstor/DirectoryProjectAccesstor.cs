using FastBIRe.Project.Models;
#if !NETSTANDARD2_0
using System.Text.Json;
#endif

namespace FastBIRe.Project.Accesstor
{
#if !NETSTANDARD2_0
    public class JsonDirectoryProjectAccesstor : JsonDirectoryProjectAccesstor<Project<string>>
    {
        public JsonDirectoryProjectAccesstor(string path, string extensions) : base(path, extensions)
        {
        }
    }
    public class JsonDirectoryProjectAccesstor<TProject> : JsonDirectoryProjectAccesstor<TProject, IProjectAccesstContext<string>, string>
           where TProject : IProject<string>
    {
        public JsonDirectoryProjectAccesstor(string path, string extensions) : base(path, extensions)
        {
        }
    }
    public class JsonDirectoryProjectAccesstor<TProject, TInput, TId> : DirectoryProjectAccesstor<TInput, TProject, TId>
        where TInput : IProjectAccesstContext<TId>
        where TProject : IProject<TId>
    {
        public JsonDirectoryProjectAccesstor(string path, string extensions) : base(path, extensions)
        {
        }

        public JsonSerializerOptions? Options { get; set; }

        public override TProject? ConvertToProject(string file)
        {
            using (var fs = File.OpenRead(file))
            {
                return JsonSerializer.Deserialize<TProject>(fs, Options);
            }
        }

        public override async Task WriteProjectToFileAsync(string file, TProject project, CancellationToken cancellationToken = default)
        {
            using (var fs = File.Open(file, FileMode.Create))
            {
                await JsonSerializer.SerializeAsync(fs, project, Options, cancellationToken);
            }
        }
    }
#endif

    public abstract class DirectoryProjectAccesstor<TInput, TProject, TId> : ProjectAccesstorBase<TInput, TProject, TId>
        where TProject : IProject<TId>
        where TInput : IProjectAccesstContext<TId>
    {
        protected DirectoryProjectAccesstor(string path, string extensions)
        {
            Root = path ?? throw new ArgumentNullException(nameof(path));
            var dir = new DirectoryInfo(path);
            if (!dir.Exists)
            {
                dir.Create();
            }
            Extensions = extensions ?? throw new ArgumentNullException(nameof(extensions));
        }

        public string Root { get; }

        public string Extensions { get; }

        public SearchOption SearchOption { get; set; } = SearchOption.AllDirectories;

        public abstract TProject? ConvertToProject(string file);

        public abstract Task WriteProjectToFileAsync(string file, TProject project, CancellationToken cancellationToken = default);

        public virtual string GetFilePath(TInput input)
        {
            return Path.Combine(Root, $"{input.Id}.{Extensions}");
        }

        public virtual IEnumerable<string> EnumerableProjectFile(string? keyword)
        {
            return Directory.EnumerateFiles(Root, !string.IsNullOrEmpty(keyword) ? $"*{keyword}*.{Extensions}" : $"*.{Extensions}", SearchOption);
        }

        public override Task<IReadOnlyList<TProject>> AllProjectsAsync(TInput? input, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var res = EnumerableProjectFile(input?.ToString()).Select(ConvertToProject).Where(x => x != null).ToList();
            return Task.FromResult<IReadOnlyList<TProject>>(res!);
        }

        protected override Task<int> OnCleanProjectAsync(TInput? input, CancellationToken cancellationToken = default)
        {
            var del = 0;
            foreach (var item in EnumerableProjectFile(null))
            {
                File.Delete(item);
                del++;
            }
            return Task.FromResult(del);
        }

        protected override async Task<bool> OnCreateProjectAsync(TInput input, TProject project, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = GetFilePath(input);
            var dir = new DirectoryInfo(Path.GetDirectoryName(path)!);
            if (!dir.Exists)
            {
                dir.Create();
            }
            await WriteProjectToFileAsync(path, project, cancellationToken);
            return true;
        }
        protected override Task<bool> OnUpdateProjectAsync(TInput input, TProject project, CancellationToken cancellationToken = default)
        {
            return OnCreateProjectAsync(input, project, cancellationToken);
        }
        protected override Task<bool> OnDeleteProjectAsync(TInput input, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = GetFilePath(input);
            if (File.Exists(path))
            {
                File.Delete(path);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        protected override Task<TProject?> OnGetProjectAsync(TInput input, CancellationToken cancellationToken = default)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            var path = GetFilePath(input);
            return Task.FromResult(ConvertToProject(path));
        }

        public override Task<bool> ProjectExistsAsync(TInput input, CancellationToken cancellationToken = default)
        {
            var path = GetFilePath(input);
            return Task.FromResult(File.Exists(path));
        }
    }
}
