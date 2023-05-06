using FastBIRe.Project.Models;
using System.Security.Cryptography;
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
    public class JsonDirectoryProjectAccesstor<TProject, TInput, TId> : DirectoryProjectAccesstor<TInput, TId>
        where TInput : IProjectAccesstContext<TId>
        where TProject : IProject<TId>
    {
        public JsonDirectoryProjectAccesstor(string path, string extensions) : base(path, extensions)
        {
        }

        public JsonSerializerOptions? Options { get; set; }

        public override IProject<TId>? ConvertToProject(string file)
        {
            using (var fs = File.OpenRead(file))
            {
                return JsonSerializer.Deserialize<TProject>(fs, Options);
            }
        }

        public override async Task WriteProjectToFileAsync(string file, IProject<TId> project, CancellationToken cancellationToken = default)
        {
            using (var fs = File.Open(file, FileMode.Create))
            {
                await JsonSerializer.SerializeAsync(fs, project, Options, cancellationToken);
            }
        }
    }
#endif
    public abstract class DirectoryProjectAccesstor<TInput, TId> : IProjectAccesstor<TInput, TId>
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

        public abstract IProject<TId>? ConvertToProject(string file);

        public abstract Task WriteProjectToFileAsync(string file, IProject<TId> project, CancellationToken cancellationToken = default);

        public virtual string GetFilePath(TInput input)
        {
            return Path.Combine(Root, $"{input.Id}.{Extensions}");
        }

        public virtual IEnumerable<string> EnumerableProjectFile(string? keyword)
        {
            return Directory.EnumerateFiles(Root, keyword == null ? $"*{keyword}*.{Extensions}" : $"*.{Extensions}", SearchOption.AllDirectories);
        }

        public virtual Task<IReadOnlyList<IProject<TId>>> AllProjectsAsync(TInput? input, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var res = EnumerableProjectFile(null).Select(ConvertToProject).Where(x => x != null).ToList();
            return Task.FromResult<IReadOnlyList<IProject<TId>>>(res!);
        }

        public virtual Task<int> CleanProjectAsync(TInput? input,CancellationToken cancellationToken = default)
        {
            var del = 0;
            foreach (var item in EnumerableProjectFile(null))
            {
                File.Delete(item);
                del++;
            }
            return Task.FromResult(del);
        }

        public virtual async Task<bool> CreateProjectAsync(TInput input, IProject<TId> project, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = GetFilePath(input);
            var dir = new DirectoryInfo(Path.GetDirectoryName(path)!);
            if (!dir.Exists)
            {
                dir.Create();
            }
            await WriteProjectToFileAsync(path, project,cancellationToken);
            return true;
        }

        public virtual Task<bool> DeleteProjectAsync(TInput input, CancellationToken cancellationToken = default)
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

        public virtual Task<IProject<TId>?> GetProjectAsync(TInput input, CancellationToken cancellationToken = default)
        {
            var path = GetFilePath(input);
            return Task.FromResult(ConvertToProject(path));
        }

        public virtual Task<bool> ProjectExistsAsync(TInput input, CancellationToken cancellationToken = default)
        {
            var path = GetFilePath(input);
            return Task.FromResult(File.Exists(path));
        }
    }
}
