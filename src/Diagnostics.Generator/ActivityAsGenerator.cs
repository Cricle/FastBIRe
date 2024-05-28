using Diagnostics.Generator.Internal;
using Microsoft.CodeAnalysis;

namespace Diagnostics.Generator
{
    [Generator]
    public class ActivityAsGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider = context.SyntaxProvider.ForAttributeWithMetadataName(Consts.ActivityAsAttribute.FullName,
                ActivityAsParser.Predicate,
                ActivityAsParser.Transform);
            context.RegisterSourceOutput(provider, Execute);
        }
        private void Execute(SourceProductionContext context, GeneratorTransformResult<ISymbol> node)
        {
            var parser = new ActivityAsParser();
            parser.Execute(context, node);
        }
    }
}
