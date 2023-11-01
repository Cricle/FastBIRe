using System.Linq.Expressions;

namespace FastBIRe.Building
{
    public class BinaryMetadata : QueryMetadata, IEquatable<BinaryMetadata>, IBinaryMetadata
    {
        public BinaryMetadata(object left, ExpressionType expressionType, object right)
        {
            Left = new ValueMetadata(left);
            ExpressionType = expressionType;
            Right = new ValueMetadata(right);
        }
        public BinaryMetadata(IQueryMetadata left, ExpressionType expressionType, IQueryMetadata right)
        {
            Left = left;
            ExpressionType = expressionType;
            Right = right;
        }

        public IQueryMetadata Left { get; }

        public ExpressionType ExpressionType { get; }

        public IQueryMetadata Right { get; }

        public override IEnumerable<IQueryMetadata> GetChildren()
        {
            yield return Left;
            yield return Right;
        }

        protected virtual string Middle(string op)
        {
            return string.Concat(LeftToString(), " ", op, " ", RightToString());
        }
        protected virtual string ArrayIndex()
        {
            return string.Concat(LeftToString(), "[", RightToString(), "]");
        }

        public virtual string? LeftToString()
        {
            return Left?.ToString() ?? "null";
        }
        public virtual string? RightToString()
        {
            return Right?.ToString() ?? "null";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Left, ExpressionType, Right);
        }
        public override bool Equals(object? obj)
        {
            return Equals(obj as BinaryMetadata);
        }


        public virtual string GetToken()
        {
            if (OperatorHelpers.TokenMap.TryGetValue(ExpressionType, out var tk))
            {
                return tk.Token;
            }
            throw new NotSupportedException(ExpressionType.ToString());
        }

        public override string ToString()
        {
            if (ExpressionType == ExpressionType.ArrayIndex)
            {
                return ArrayIndex();
            }
            var token = GetToken();
            return Middle(token);
        }

        public bool Equals(BinaryMetadata? other)
        {
            if (other == null)
            {
                return false;
            }
            return CheckLeftEquals(other.Left) && CheckRightEquals(other.Right) && other.ExpressionType == ExpressionType;
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
        protected virtual bool CheckRightEquals(in IQueryMetadata otherRight)
        {
            if (otherRight == null && Right == null)
            {
                return true;
            }
            if (otherRight == null || Right == null)
            {
                return false;
            }
            return otherRight.Equals(Right);
        }

    }
}
