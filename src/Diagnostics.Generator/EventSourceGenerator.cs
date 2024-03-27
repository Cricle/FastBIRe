using Diagnostics.Generator.Internal;
using Microsoft.CodeAnalysis;

namespace Diagnostics.Generator
{
    [Generator]
    public class EventSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider = context.SyntaxProvider.ForAttributeWithMetadataName(Consts.EventSourceGenerateAttribute.FullName,
                EventSourceParser.Predicate,
                EventSourceParser.Transform);
            context.RegisterSourceOutput(provider, Execute);
        }
        private void Execute(SourceProductionContext context, GeneratorTransformResult<ISymbol> node)
        {
            var parser = new EventSourceParser();
            parser.Execute(context, node);
        }
    }
}
