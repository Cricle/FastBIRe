using FastBIRe.ModelGen.Internal;
using Microsoft.CodeAnalysis;

namespace FastBIRe.ModelGen
{
    [Generator]
    public class ModelGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider = context.SyntaxProvider.ForAttributeWithMetadataName(Consts.GenerateModelAttribute.FullName,
                ModelParser.Predicate,
                ModelParser.Transform);
            context.RegisterSourceOutput(provider, Execute);
        }
        private void Execute(SourceProductionContext context, GeneratorTransformResult<ISymbol> node)
        {
            var parser = new ModelParser();
            parser.Execute(context, node);
        }
    }
}
