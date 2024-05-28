using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

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
        public static bool HasKeyword(ISymbol symbol, SyntaxKind kind)
        {
            var syntax = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
            if (symbol == null)
            {
                return false;
            }
            if (syntax is MemberDeclarationSyntax m)
            {
                return m.Modifiers.Any(x => x.IsKind(kind));
            }
            if (syntax is ClassDeclarationSyntax c)
            {
                return c.Modifiers.Any(x => x.IsKind(kind));
            }
            if (syntax is StructDeclarationSyntax s)
            {
                return s.Modifiers.Any(x => x.IsKind(kind));
            }
            return false;
        }
    }
}
