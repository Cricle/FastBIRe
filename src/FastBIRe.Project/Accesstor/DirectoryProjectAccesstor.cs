using FastBIRe.Project.Models;

namespace FastBIRe.Project.Accesstor
{
    public abstract class DirectoryProjectAccesstor<TInput, TId> : IProjectAccesstor<TInput, TId>
        where TInput : IProjectAccesstContext<TId>
    {
        protected DirectoryProjectAccesstor(string path)
        {
            Root = path;
            var dir = new DirectoryInfo(path);
            if (!dir.Exists)
            {
                dir.Create();
            }
        }

        public string Root { get; }

        public abstract IProject<TId> ConvertToProject(string file);

        public abstract Task WriteProjectToFileAsync(string file, IProject<TId> project);

        public abstract string GetFilePath(TInput input);

        public virtual IEnumerable<string> EnumerableProjectFile(string? keyword)
        {
            return Directory.EnumerateFiles(Root, keyword == null ? $"*{keyword}*" : "*", SearchOption.AllDirectories);
        }

        public virtual Task<IReadOnlyList<IProject<TId>>> AllProjectsAsync(TInput input, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var res = EnumerableProjectFile(null).Select(ConvertToProject).ToList();
            return Task.FromResult<IReadOnlyList<IProject<TId>>>(res);
        }

        public virtual Task<int> CleanProjectAsync(CancellationToken cancellationToken = default)
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
            await WriteProjectToFileAsync(path, project);
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

        public virtual Task<IProject<TId>> GetProjectAsync(TInput input, CancellationToken cancellationToken = default)
        {
            var path = GetFilePath(input);
            return Task.FromResult(ConvertToProject(path));
        }

        public virtual Task<bool> HasProjectAsync(TInput input, IProject<TId> project, CancellationToken cancellationToken = default)
        {
            var path = GetFilePath(input);
            return Task.FromResult(File.Exists(path));
        }
    }
}
