using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Diagnostics.Generator.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class WriteEventBodyAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Messages.WriteEventBody);

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
            var isBaseOnEventSource = false;
            foreach (var item in node.BaseList.Types)
            {
                var symbol = context.SemanticModel.GetSymbolInfo(item.Type).Symbol;
                if (SymbolEqualityComparer.Default.Equals(symbol, eventSourceSymbol))
                {
                    //Base on eventsource
                    isBaseOnEventSource = true;
                    break;
                }
            }
            if (!isBaseOnEventSource)
            {
                return;
            }
            var methods = node.Members.OfType<MethodDeclarationSyntax>().ToList();
            var eventAttributeSymbol=SymbolHelper.GetEventAttributeSymbol(context.SemanticModel.Compilation);
            //Debugger.Launch();
            foreach (var method in methods)
            {
                if (method.AttributeLists.Count == 0 && method.Body != null && method.Body.Statements.Count != 0)
                {
                    continue;
                }
                foreach (var attr in method.AttributeLists)
                {
                    if (attr.Attributes.Any(x=> SymbolEqualityComparer.Default.Equals(context.SemanticModel.GetSymbolInfo(x).Symbol?.ContainingType, eventAttributeSymbol)))
                    {
                        if (method.Body == null || method.Body.Statements.Count == 0)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Messages.WriteEventBody, method.GetLocation()));
                        }
                    }
                }
            }
        }
    }
}
