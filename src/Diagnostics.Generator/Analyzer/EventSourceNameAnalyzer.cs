using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Diagnostics.Generator.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EventSourceNameAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Messages.NameNeedEndWithEventSource);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
        }
        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var node = (ClassDeclarationSyntax)context.Node;
            if (node.BaseList == null || node.BaseList.Types.Count == 0)
            {
                return;
            }
            var eventSourceSymbol = SymbolHelper.GetEventSourceSymbol(context.SemanticModel.Compilation);
            foreach (var item in node.BaseList.Types)
            {
                var symbol=context.SemanticModel.GetSymbolInfo(item.Type).Symbol;
                if (SymbolEqualityComparer.Default.Equals(symbol, eventSourceSymbol))
                {
                    if (!node.Identifier.ValueText.EndsWith("EventSource"))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Messages.NameNeedEndWithEventSource, node.GetLocation()));
                    }
                }
            }
        }
    }
}
