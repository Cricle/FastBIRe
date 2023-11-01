namespace FastBIRe.Building
{
    public interface IBinaryMetadata : IExpressionTypeProvider, IQueryMetadata
    {
        IQueryMetadata Left { get; }

        IQueryMetadata Right { get; }
    }
}
