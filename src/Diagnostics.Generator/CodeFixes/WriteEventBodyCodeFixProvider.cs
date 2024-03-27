using Diagnostics.Generator.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.Generator.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(WriteEventBodyCodeFixProvider)), Shared]
    public class WriteEventBodyCodeFixProvider : CodeFixProvider
    {
        const string EventDatasName = "datas";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(Messages.WriteEventBody.Id);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First(x => x.Id == Messages.WriteEventBody.Id);
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var declaration = root?.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (declaration == null)
            {
                return;
            }
            var model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            if (model==null)
            {
                return;
            }
            var eventSymbol = SymbolHelper.GetEventAttributeSymbol(model.Compilation);
            //Debugger.Launch();
            foreach (var item in declaration.AttributeLists.SelectMany(x => x.Attributes))
            {
                var symbolItem = model.GetSymbolInfo(item).Symbol?.ContainingType;
                if (SymbolEqualityComparer.Default.Equals(eventSymbol,symbolItem))
                {
                    var stringSymbol = SymbolHelper.GetStringSymbol(model.Compilation);
                    var eventId = int.Parse(item.ArgumentList!.Arguments[0].ToString());
                    context.RegisterCodeFix(CodeAction.Create(
                        title: "Write event body",
                        createChangedDocument: c => GenerateBody(context.Document, declaration, eventId, stringSymbol, model, c),
                        equivalenceKey: nameof(EventSourceNameCodeFixProvider)
                        ),
                        diagnostic);
                }
            }
        }
        private static async Task<Document> GenerateBody(Document document,
            MethodDeclarationSyntax methodDeclaration,
            int eventId,
            INamedTypeSymbol stringSymbol,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            var args = methodDeclaration.ParameterList.Parameters;
            var statements = new List<StatementSyntax>();
            if (args.Count == 0)
            {
                statements.Add(WriteEventBodyHelper.GenerateWriteEvent(eventId));
            }
            else
            {
                statements.Add(WriteEventBodyHelper.GenerateDeclare(EventDatasName, args.Count));
                for (int i = 0; i < args.Count; i++)
                {
                    var item = args[i];

                    if (SymbolEqualityComparer.Default.Equals(semanticModel.GetSymbolInfo(item.Type!).Symbol, stringSymbol))
                    {
                        statements.Add(WriteEventBodyHelper.GenerateStringAssign(EventDatasName, item.Identifier.ValueText, i));
                    }
                    else
                    {
                        statements.Add(WriteEventBodyHelper.GenerateStructAssign(EventDatasName, item.Identifier.ValueText, i));
                    }
                }
                statements.Add(WriteEventBodyHelper.GenerateWriteEvent(EventDatasName,eventId, args.Count));
            }

            var block = SyntaxFactory.Block(statements);
            var oldNode = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var newMethod = methodDeclaration.WithBody(block)
                .WithModifiers(SyntaxFactory.TokenList(methodDeclaration.Modifiers.Where(x => !x.IsKind(SyntaxKind.PartialKeyword)).ToArray()))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None));
            var newNode = oldNode.ReplaceNode(methodDeclaration, newMethod);

            return document.WithSyntaxRoot(newNode!);
        }

    }
}
