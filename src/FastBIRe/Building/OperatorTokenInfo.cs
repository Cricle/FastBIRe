using System.Linq.Expressions;

namespace FastBIRe.Building
{
    public class OperatorTokenInfo : IEquatable<OperatorTokenInfo>
    {
        public OperatorTokenInfo(ExpressionType expressionType, string token, OperatorTokenPlacement placement)
        {
            ExpressionType = expressionType;
            Token = token ?? throw new ArgumentNullException(nameof(token));
            Placement = placement;
        }

        public ExpressionType ExpressionType { get; }

        public string Token { get; }

        public OperatorTokenPlacement Placement { get; }

        public override string ToString()
        {
            return $"Type:{ExpressionType}, Token:{Token}, Placement:{Placement}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ExpressionType, Token, Placement);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as OperatorTokenInfo);
        }

        public bool Equals(OperatorTokenInfo? other)
        {
            if (other == null)
            {
                return false;
            }
            return other.ExpressionType == ExpressionType &&
                other.Token == Token &&
                other.Placement == Placement;
        }
    }
}
