using FastBIRe.Functions;
using System.Linq.Expressions;

namespace FastBIRe.Building
{
    public class MethodMetadata : QueryMetadata, IEquatable<MethodMetadata>, IMethodMetadata
    {
        public MethodMetadata(SQLFunctions function, params IQueryMetadata[] args)
        {
            Function = function;
            Method = string.Empty;
            Args = args ?? Array.Empty<IQueryMetadata>();
        }
        public MethodMetadata(string method, params IQueryMetadata[] args)
        {
            Method = method;
            Args = args ?? Array.Empty<IQueryMetadata>();
        }

        public ExpressionType ExpressionType { get; } = ExpressionType.Call;

        public string Method { get; }

        public IReadOnlyList<IQueryMetadata> Args { get; }

        public SQLFunctions? Function { get; }

        public override int GetHashCode()
        {
            var hs = new HashCode();
            hs.Add(Function);
            hs.Add(Method);
            for (int i = 0; i < Args.Count; i++)
            {
                hs.Add(Args[i]);
            }
            return hs.ToHashCode();
        }
        public override IEnumerable<IQueryMetadata> GetChildren()
        {
            return Args;
        }
        public override bool Equals(object? obj)
        {
            return Equals(obj as MethodMetadata);
        }
        protected virtual string ToString(IQueryMetadata arg)
        {
            return arg?.ToString() ?? "null";
        }
        public override string ToString()
        {
            return $"{Method}({string.Join(",", Args)})";
        }

        public bool Equals(MethodMetadata? other)
        {
            if (other == null)
            {
                return false;
            }
            return other.Method == Method &&
                other.Function == Function &&
                Args.Count == other.Args.Count &&
                Args.SequenceEqual(other.Args);
        }
    }
}
