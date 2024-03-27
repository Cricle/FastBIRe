using System.Linq.Expressions;

namespace FastBIRe.Building
{
    public interface IExpressionTypeProvider
    {
        ExpressionType ExpressionType { get; }
    }
}
