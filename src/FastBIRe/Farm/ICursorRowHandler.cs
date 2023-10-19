using FastBIRe.Data;

namespace FastBIRe.Farm
{
    public class CopyCursorRowHandlerResult<TKey> : CursorRowHandlerResult
    {
        public CopyCursorRowHandlerResult(long currentPoint, long affectedCount, long round, IList<RowWriteResult<TKey>> rowWriteResults)
            : base(currentPoint, affectedCount, round)
        {
            RowWriteResults = rowWriteResults;
        }

        public IList<RowWriteResult<TKey>> RowWriteResults { get; }
    }
    public class CursorRowHandlerResult : ICursorRowHandlerResult
    {
        public CursorRowHandlerResult(long currentPoint, long affectedCount, long round)
        {
            CurrentPoint = currentPoint;
            AffectedCount = affectedCount;
            Round = round;
        }

        public long CurrentPoint { get; }

        public long AffectedCount { get; }

        public long Round { get; }
    }
    public interface ICursorRowHandlerResult
    {
        public long CurrentPoint { get; }

        public long AffectedCount { get; }

        public long Round { get; }
    }
    public interface ICursorRowHandler
    {
        Task<ICursorRowHandlerResult> HandlerCursorRowAsync(CursorRow rows, CancellationToken token = default);
    }
}
