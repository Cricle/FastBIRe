using Diagnostics.Generator.Internal;
using Microsoft.CodeAnalysis;

namespace Diagnostics.Generator
{
    [Generator]
    public class MeterMethodGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider = context.SyntaxProvider.ForAttributeWithMetadataName(Consts.MeterGenerateAttribute.FullName,
                MeterMethodParser.Predicate,
                MeterMethodParser.Transform);
            context.RegisterSourceOutput(provider, Execute);
        }
        private void Execute(SourceProductionContext context, GeneratorTransformResult<ISymbol> node)
        {
            var parser = new MeterMethodParser();
            parser.Execute(context, node);
        }
    }
}
