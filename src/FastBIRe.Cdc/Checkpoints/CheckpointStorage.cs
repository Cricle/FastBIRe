using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FastBIRe.Cdc.Checkpoints
{
    public interface ICheckpointStorage
    {
        Task<int> CountAsync(string? databaseName, CancellationToken token = default);

        Task<CheckpointPackage?> GetAsync(string databaseName, string tableName, CancellationToken token = default);

        Task<IList<CheckpointPackage>> GetAsync(string databaseName, CancellationToken token = default);

        Task<IList<CheckpointPackage>> GetAllAsync(CancellationToken token = default);

        Task<bool> SetAsync(CheckpointPackage package, CancellationToken token = default);

        Task<int> SetAsync(IEnumerable<CheckpointPackage> packages, CancellationToken token = default);

        Task<bool> RemoveAsync(CheckpointPackage package, CancellationToken token = default);

        Task<int> RemoveAsync(IEnumerable<CheckpointPackage> packages, CancellationToken token = default);

        Task<int?> CleanAsync(CancellationToken token = default);
    }
}
