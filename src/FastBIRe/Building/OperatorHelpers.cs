using System.Linq.Expressions;

namespace FastBIRe.Building
{
    public static class OperatorHelpers
    {
        public static readonly IReadOnlyList<OperatorTokenInfo> DefaultTokenMap = new OperatorTokenInfo[]
        {
             new OperatorTokenInfo(ExpressionType.Add, "+", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.AddChecked, "+", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.And, "&", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.AndAlso, "&&", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.Coalesce, "??", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.Divide, "/", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.Equal, "==", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.ExclusiveOr, "^", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.GreaterThan, ">", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.GreaterThanOrEqual, ">=", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.LeftShift, "<<", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.LessThan, "<", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.LessThanOrEqual, "<=", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.Modulo, "%", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.Multiply, "*", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.MultiplyChecked, "*", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.NotEqual, "!=", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.Or, "|", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.OrElse, "||", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.RightShift, ">>", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.Subtract, "-", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.SubtractChecked, "-", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.TypeAs, "as", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.TypeIs, "is", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.Assign, "=", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.AddAssign, "+=", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.AndAssign, "&=", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.DivideAssign, "/=", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.ExclusiveOrAssign, "^=", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.LeftShiftAssign, "<<=", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.ModuloAssign, "%=", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.MultiplyAssign, "*=", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.OrAssign, "|=", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.RightShiftAssign, ">>=", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.SubtractAssign, "-=", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.AddAssignChecked, "+=", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.MultiplyAssignChecked, "*=", OperatorTokenPlacement.Middle),
             new OperatorTokenInfo(ExpressionType.SubtractAssignChecked, "-=", OperatorTokenPlacement.Middle),

             new OperatorTokenInfo(ExpressionType.UnaryPlus, "+", OperatorTokenPlacement.Left),
             new OperatorTokenInfo(ExpressionType.Negate, "-", OperatorTokenPlacement.Left),
             new OperatorTokenInfo(ExpressionType.NegateChecked, "-", OperatorTokenPlacement.Left),
             new OperatorTokenInfo(ExpressionType.Decrement, "-1", OperatorTokenPlacement.Right),
             new OperatorTokenInfo(ExpressionType.Increment, "+1", OperatorTokenPlacement.Right),
             new OperatorTokenInfo(ExpressionType.PreIncrementAssign, "++", OperatorTokenPlacement.Left),
             new OperatorTokenInfo(ExpressionType.PreDecrementAssign, "--", OperatorTokenPlacement.Left),
             new OperatorTokenInfo(ExpressionType.PostIncrementAssign, "++", OperatorTokenPlacement.Right),
             new OperatorTokenInfo(ExpressionType.PostDecrementAssign, "--", OperatorTokenPlacement.Right),
             new OperatorTokenInfo(ExpressionType.Not, "~", OperatorTokenPlacement.Left),
             new OperatorTokenInfo(ExpressionType.Not, "!", OperatorTokenPlacement.Left),
             new OperatorTokenInfo(ExpressionType.IsTrue, "is true", OperatorTokenPlacement.Right),
             new OperatorTokenInfo(ExpressionType.IsFalse, "is false", OperatorTokenPlacement.Right),
             new OperatorTokenInfo(ExpressionType.Default, "", OperatorTokenPlacement.Right),

        };

        public static readonly IReadOnlyDictionary<string, OperatorTokenInfo> TokenStringMap;
        public static readonly IReadOnlyDictionary<ExpressionType, OperatorTokenInfo> TokenMap;

        static OperatorHelpers()
        {
            var tokenMap = new Dictionary<ExpressionType, OperatorTokenInfo>();
            var tokenStringMap = new Dictionary<string, OperatorTokenInfo>(StringComparer.OrdinalIgnoreCase);

            TokenStringMap = tokenStringMap;
            TokenMap = tokenMap;
            foreach (var item in DefaultTokenMap)
            {
                tokenMap[item.ExpressionType] = item;
                tokenStringMap[item.Token] = item;
            }
        }

    }
}
