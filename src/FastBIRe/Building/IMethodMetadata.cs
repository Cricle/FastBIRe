using FastBIRe.Functions;

namespace FastBIRe.Building
{
    public interface IMethodMetadata : IQueryMetadata, IExpressionTypeProvider
    {
        string Method { get; }

        SQLFunctions? Function { get; }

        IReadOnlyList<IQueryMetadata>? Args { get; }
    }
}
