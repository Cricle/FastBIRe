using Diagnostics.Generator.Internal;
using Microsoft.CodeAnalysis;

namespace Diagnostics.Generator
{
    [Generator]
    public class CounterMappingGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider = context.SyntaxProvider.ForAttributeWithMetadataName(Consts.CounterMappingAttribute.FullName,
                CounterMappingParser.Predicate,
                CounterMappingParser.Transform);
            context.RegisterSourceOutput(provider, Execute);
        }
        private void Execute(SourceProductionContext context, GeneratorTransformResult<ISymbol> node)
        {
            var parser = new CounterMappingParser();
            parser.Execute(context, node);
        }
    }
}
