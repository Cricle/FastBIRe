using Microsoft.CodeAnalysis;

namespace Diagnostics.Generator.Internal
{
    internal abstract class ParserBase
    {
        public static string GetSpecialName(string name)
        {
            var specialName = name.TrimStart('_');
            return char.ToUpper(specialName[0]) + specialName.Substring(1);
        }

        public static string GetVisiblity(ISymbol symbol)
        {
            return GeneratorTransformResult<ISymbol>.GetAccessibilityString(symbol.DeclaredAccessibility);
        }
    }
}
