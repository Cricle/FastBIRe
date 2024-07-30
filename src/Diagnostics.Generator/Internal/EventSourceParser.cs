using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.Threading;

namespace Diagnostics.Generator.Internal
{
    internal class EventSourceParser : ParserBase
    {
        public void Execute(SourceProductionContext context, GeneratorTransformResult<ISymbol> node)
        {
            var symbol = (INamedTypeSymbol)node.SyntaxContext.TargetSymbol;
            var methods = symbol.GetMembers().OfType<IMethodSymbol>()
                .Where(x => x.HasAttribute(Consts.EventAttribute.FullName) && HasKeyword(x, SyntaxKind.PartialKeyword));
            if (EventSourceHelper.TryWriteCode(context, node.SemanticModel, symbol,symbol, false, methods, out var code))
            {
                code = Helpers.FormatCode(code!);
                context.AddSource($"{symbol.Name}.g.cs", code);
            }
        }

        public static bool Predicate(SyntaxNode node, CancellationToken token)
        {
            return true;
        }
        public static GeneratorTransformResult<ISymbol> Transform(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            return new GeneratorTransformResult<ISymbol>(context.TargetSymbol, context);
        }
    }
}
