using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace FastBIRe
{
    public static class ParamterParser
    {
        public static readonly ConcurrentDictionary<Type, IParamterParser> paramterParsers = new ConcurrentDictionary<Type, IParamterParser>();

        public static void Set(Type type, IParamterParser parser)
        {
            paramterParsers.AddOrUpdate(type, _ => parser, (_, __) => parser);
        }
        public static bool Contains(Type type)
        {
            return paramterParsers.ContainsKey(type);
        }

        public static IEnumerable<KeyValuePair<string, object?>> Parse(
#if NET7_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
            object? value)
        {
            if (value == null)
            {
                return Enumerable.Empty<KeyValuePair<string, object?>>();
            }
            return Get(value.GetType()).Parse(value);
        }

        public static IParamterParser Get(
#if NET7_0_OR_GREATER
            [DynamicallyAccessedMembers( DynamicallyAccessedMemberTypes.All)]
#endif
            Type type)
        {
            if (paramterParsers.TryGetValue(type, out var parser))
            {
                return parser;
            }
            return DefaultParamterParser.Instance;
        }
    }
}
