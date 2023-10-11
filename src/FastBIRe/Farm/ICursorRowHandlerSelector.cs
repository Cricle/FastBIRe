namespace FastBIRe.Farm
{
    public interface ICursorRowHandlerSelector
    {
        ICursorRowHandler GetHandler(CursorRow row);
    }
}
