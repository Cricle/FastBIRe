namespace FastBIRe.Building
{
    public interface IUnaryMetadata : IExpressionTypeProvider
    {
        IQueryMetadata Left { get; }
    }
}
