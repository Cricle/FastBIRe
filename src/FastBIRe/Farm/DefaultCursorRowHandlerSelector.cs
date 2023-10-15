namespace FastBIRe.Farm
{
    public class DefaultCursorRowHandlerSelector : ICursorRowHandlerSelector
    {
        public static DefaultCursorRowHandlerSelector Single(IDbScriptExecuter sourceConnection, IDbScriptExecuter destConnection, string tableName)
        {
            return new DefaultCursorRowHandlerSelector(_ => DefaultCursorRowHandler.FromDefault(sourceConnection, destConnection, tableName));
        }

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
