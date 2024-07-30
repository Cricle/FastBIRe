using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Diagnostics.Generator.Internal
{
    internal class MapToEventSourceParser : ParserBase
    {
        public void Execute(SourceProductionContext context, GeneratorTransformResult<ISymbol> node)
        {
            //Debugger.Launch();
            var symbol = (INamedTypeSymbol)node.SyntaxContext.TargetSymbol;
            var eventSourceSymbol = (INamedTypeSymbol)symbol.GetAttribute(Consts.MapToEventSourceAttribute.FullName)!.GetByIndex<ISymbol>(0)!;

            var methods = symbol.GetMembers().OfType<IMethodSymbol>()
                .Where(x => x.HasAttribute(Consts.EventAttribute.FullName) && x.HasAttribute(Consts.LoggerMessageAttribute.FullName));

            //Debugger.Launch();
            if (EventSourceHelper.TryWriteCode(context, node.SemanticModel, eventSourceSymbol,symbol, true, methods, out var code))
            {
                code = Helpers.FormatCode(code!);
                context.AddSource($"{eventSourceSymbol.Name}.FromLog.g.cs", code);

                var mapToActivityAttr = symbol.GetAttribute(Consts.MapToActivityAttribute.FullName);
                //Debugger.Launch();
                if (mapToActivityAttr != null)
                {
                    var activitySymbol = mapToActivityAttr.GetByIndex<ISymbol>(0);
                    var attr = symbol.GetAttribute(Consts.MapToActivityAttribute.FullName);
                    if (ActivityMapParseHelper.TryWriteActivityMapCode(context, attr, eventSourceSymbol, methods, node.SemanticModel, true, out var mapToActivityCode))
                    {
                        context.AddSource($"{activitySymbol!.Name}.LogActivityMap.g.cs", mapToActivityCode!);
                    }
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
