using System.Data;
using System.Data.Common;

namespace FastBIRe
{
    public interface IDbScriptTransaction
    {
        DbTransaction? Transaction { get; }

        void BeginTransaction(IsolationLevel level = IsolationLevel.Unspecified);

        Task BeginTransactionAsync(IsolationLevel level = IsolationLevel.Unspecified, CancellationToken token = default);

        void Commit();

        Task CommitAsync(CancellationToken token = default);

        void Rollback();

        Task RollbackAsync(CancellationToken token = default);
    }
}
