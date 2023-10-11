namespace FastBIRe.Farm
{
    public class DefaultCursorRowHandlerSelector : ICursorRowHandlerSelector
    {
        public DefaultCursorRowHandlerSelector(Func<CursorRow, ICursorRowHandler> handlerGetter)
        {
            HandlerGetter = handlerGetter;
        }

        public Func<CursorRow, ICursorRowHandler> HandlerGetter { get; }

        public ICursorRowHandler GetHandler(CursorRow row)
        {
            return HandlerGetter(row);
        }
    }
}
