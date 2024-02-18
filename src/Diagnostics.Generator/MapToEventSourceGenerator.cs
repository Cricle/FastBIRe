using Diagnostics.Generator.Internal;
using Microsoft.CodeAnalysis;

namespace Diagnostics.Generator
{
    [Generator]
    public class MapToEventSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider = context.SyntaxProvider.ForAttributeWithMetadataName(Consts.MapToEventSourceAttribute.FullName,
                MapToEventSourceParser.Predicate,
                MapToEventSourceParser.Transform);
            context.RegisterSourceOutput(provider, Execute);
        }
        private void Execute(SourceProductionContext context, GeneratorTransformResult<ISymbol> node)
        {
            var parser = new MapToEventSourceParser();
            parser.Execute(context, node);
        }
    }
}
