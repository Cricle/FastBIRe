using FastBIRe.Project.Accesstor;
using FastBIRe.Project.Models;
using System.Collections.Concurrent;

namespace FastBIRe.Project
{
    public abstract class ProjectFactoryBase<TInput, TId, TResult>
        where TInput : IProjectAccesstContext<TId>
        where TId : notnull
        where TResult : ProjectCreateContextResult<TId>
    {
        public ProjectFactoryBase(IProjectAccesstor<TInput, TId> projectAccesstor, IDataSchema<TInput> dataSchema)
            : this(projectAccesstor, EqualityComparer<TId>.Default, dataSchema)
        {
        }
        public ProjectFactoryBase(IProjectAccesstor<TInput, TId> projectAccesstor, IEqualityComparer<TId> equalityComparer, IDataSchema<TInput> dataSchema)
        {
            DataSchema = dataSchema ?? throw new ArgumentNullException(nameof(dataSchema));
            ProjectAccesstor = projectAccesstor ?? throw new ArgumentNullException(nameof(projectAccesstor));
            projectFirst = new ConcurrentDictionary<TId, bool>(equalityComparer);
        }
        private readonly ConcurrentDictionary<TId, bool> projectFirst;

        public IProjectAccesstor<TInput, TId> ProjectAccesstor { get; }

        public IReadOnlyDictionary<TId, bool> ProjectFirst => projectFirst;

        public bool CheckFirst { get; set; } = true;

        public IDataSchema<TInput> DataSchema { get; }

        public bool RemoveFirst(TId id)
        {
            return projectFirst.TryRemove(id, out _);
        }
        public void CleanFirst()
        {
            projectFirst.Clear();
        }
        public void DangerousSetFrist(TId id)
        {
            projectFirst.TryAdd(id, false);
        }

        public async Task<bool> CreateIfNotExistsAsync(TInput input, Func<IProject<TId>> projectCreator, CancellationToken token = default)
        {
            var exists = await ProjectAccesstor.ProjectExistsAsync(input, token);
            if (!exists)
            {
                return await ProjectAccesstor.CreateProjectAsync(input, projectCreator(), token);
            }
            return false;
        }
        public async Task<bool> DeleteIfExistsAsync(TInput input, CancellationToken token = default)
        {
            var exists = await ProjectAccesstor.ProjectExistsAsync(input, token);
            if (exists)
            {
                return await ProjectAccesstor.DeleteProjectAsync(input, token);
            }
            return false;
        }
        public async Task<TResult?> CreateDbContextAsync(TInput input, CancellationToken token = default)
        {
            var project = await ProjectAccesstor.GetProjectAsync(input, token);
            if (project != null)
            {
                var isFirst = false;
                if (CheckFirst && projectFirst.TryAdd(input.Id, false))
                {
                    isFirst = true;
                }
                try
                {
                    return await OnCreateResultAsync(input, project, isFirst, token);
                }
                catch (Exception)
                {
                    try
                    {
                        projectFirst.TryRemove(input.Id, out _);
                    }
                    catch (Exception) { }
                    throw;
                }
            }
            return null;
        }

        protected abstract Task<TResult?> OnCreateResultAsync(TInput input, IProject<TId> project, bool isFirst, CancellationToken token = default);
    }

}
