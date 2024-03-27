using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace FastBIRe.ModelGen.Internal
{
    internal static class SymbolGetExtensions
    {
        public static NullableContext GetNullableContext(this ISymbol symbol,SemanticModel model)
        {
            return model.GetNullableContext(symbol.Locations[0].SourceSpan.Start);
        }
        public static bool HasAttribute(this ISymbol symbol, string fullName)
        {
            return symbol.GetAttributes().Any(x => x.AttributeClass?.ToString() == fullName);
        }
        public static AttributeData? GetAttribute(this ISymbol symbol,string fullName)
        {
            return symbol.GetAttributes().Where(x => x.AttributeClass?.ToString() == fullName).FirstOrDefault();
        }
        public static T? GetByNamed<T>(this AttributeData data,string name)
        {
            var val = data.NamedArguments.FirstOrDefault(x => x.Key == name);
            return Cast<T>(val.Value);
        }
        public static T? GetByIndex<T>(this AttributeData data, int index)
        {
            if (data.ConstructorArguments.Length < index)
            {
                return default;
            }
            var val = data.ConstructorArguments[index];
            return Cast<T>(val);
        }

        private static T? Cast<T>(TypedConstant val)
        {
            if (val.IsNull)
            {
                return default;
            }
            if (val.Value is T t)
            {
                return t;
            }
            return (T)Convert.ChangeType(val.Value, typeof(T));
        }
    }
}
