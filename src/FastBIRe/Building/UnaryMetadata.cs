using System.Linq.Expressions;

namespace FastBIRe.Building
{
    public class UnaryMetadata : QueryMetadata, IEquatable<UnaryMetadata>, IQueryMetadata, IUnaryMetadata
    {
        public UnaryMetadata(object left, ExpressionType expressionType)
        {
            Left = new ValueMetadata(left);
            ExpressionType = expressionType;
        }
        public UnaryMetadata(IQueryMetadata left, ExpressionType expressionType)
        {
            Left = left;
            ExpressionType = expressionType;
        }

        public IQueryMetadata Left { get; }

        public ExpressionType ExpressionType { get; }

        public override IEnumerable<IQueryMetadata> GetChildren()
        {
            yield return Left;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as UnaryMetadata);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Left, ExpressionType);
        }
        public virtual string? LeftToString()
        {
            return Left?.ToString() ?? "null";
        }
        protected virtual string PreCombine(string op)
        {
            return string.Concat(op, " ", LeftToString());
        }
        protected virtual string PostCombine(string op)
        {
            return string.Concat(LeftToString(), " ", op);
        }
        public virtual string GetToken()
        {
            if (OperatorHelpers.TokenMap.TryGetValue(ExpressionType, out var tk))
            {
                return tk.Token;
            }
            throw new NotSupportedException(ExpressionType.ToString());
        }
        public bool IsPostCombine()
        {
            return !IsPreCombine();
        }
        public bool IsPreCombine()
        {
            switch (ExpressionType)
            {
                case ExpressionType.UnaryPlus:
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.PreIncrementAssign:
                case ExpressionType.PreDecrementAssign:
                case ExpressionType.PostIncrementAssign:
                case ExpressionType.OnesComplement:
                case ExpressionType.Not:
                    return true;
                default:
                    return false;
            }
        }
        public override string ToString()
        {
            var token = GetToken();
            if (IsPreCombine())
            {
                return PreCombine(token);
            }
            return PostCombine(token);
        }

        public bool Equals(UnaryMetadata? other)
        {
            if (other == null)
            {
                return false;
            }
            return CheckLeftEquals(other.Left) && other.ExpressionType == ExpressionType;
        }

        protected virtual bool CheckLeftEquals(in IQueryMetadata otherLeft)
        {
            if (otherLeft == null && Left == null)
            {
                return true;
            }
            if (otherLeft == null || Left == null)
            {
                return false;
            }
            return otherLeft.Equals(Left);
        }
    }
}
