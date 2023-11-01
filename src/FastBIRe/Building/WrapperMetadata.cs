namespace FastBIRe.Building
{
    public class WrapperMetadata : QueryMetadata, IEquatable<WrapperMetadata>
    {
        private static readonly IQueryMetadata LeftBracket = new ValueMetadata("(");
        private static readonly IQueryMetadata RightBracket = new ValueMetadata(")");

        public static WrapperMetadata Brackets(IQueryMetadata target)
        {
            return new WrapperMetadata(LeftBracket, target, RightBracket);
        }
        public WrapperMetadata(IQueryMetadata left, IQueryMetadata target, IQueryMetadata right)
        {
            Left = left;
            Target = target;
            Right = right;
        }

        public IQueryMetadata Left { get; }

        public IQueryMetadata Target { get; }

        public IQueryMetadata Right { get; }

        public override IEnumerable<IQueryMetadata> GetChildren()
        {
            yield return Left;
            yield return Target;
            yield return Right;
        }

        public override string ToString()
        {
            return Left.ToString() + Target.ToString() + Right.ToString();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Left, Target, Right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as WrapperMetadata);
        }

        public bool Equals(WrapperMetadata? other)
        {
            if (other == null)
            {
                return false;
            }
            return other.Left.Equals(Left) &&
                other.Target.Equals(Target) &&
                other.Right.Equals(Right);
        }
    }
}
