namespace FastBIRe.Farm
{
    public interface ICursorRowHandler
    {
        Task HandlerCursorRowAsync(CursorRow rows, CancellationToken token = default);
    }
}
