using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Diagnostics.Generator.Internal
{
    internal class ActivityMapParse : ParserBase
    {
        public void Execute(SourceProductionContext context, GeneratorTransformResult<ISymbol> node)
        {
            var nameTypeSymbol = (INamedTypeSymbol)node.Value;
            var eventSourceSymbol = node.SemanticModel.Compilation.GetTypeByMetadataName("System.Diagnostics.Tracing.EventSource");
            if (SymbolEqualityComparer.Default.Equals(eventSourceSymbol, nameTypeSymbol.BaseType))
            {
                var methods = nameTypeSymbol.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(x => x.HasAttribute(Consts.EventAttribute.FullName) && !x.HasAttribute(Consts.ActivityIgnoreAttribute.FullName))
                    .ToList();
                var attr = node.Value.GetAttribute(Consts.MapToActivityAttribute.FullName);
                if (ActivityMapParseHelper.TryWriteActivityMapCode(context,attr, node.SyntaxContext.TargetSymbol, methods, node.SemanticModel,false, out var code))
                {
                    context.AddSource($"{node.SyntaxContext.TargetSymbol.Name}.ActivityMap.g.cs", code!);
                }
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
