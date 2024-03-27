using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.Generator.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EventSourceNameCodeFixProvider)), Shared]
    public class EventSourceNameCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(Messages.NameNeedEndWithEventSource.Id);

        public override FixAllProvider? GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First(x => x.Id == Messages.NameNeedEndWithEventSource.Id);
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var declaration = root?.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();
            if (declaration==null)
            {
                return;
            }
            context.RegisterCodeFix(CodeAction.Create(
                title: "Add event source tail",
                createChangedDocument: c => AddEventSourceTailAsync(context.Document, declaration!, c),
                equivalenceKey: nameof(EventSourceNameCodeFixProvider)
                ), 
                diagnostic);
        }
        private static async Task<Document> AddEventSourceTailAsync(Document document,
            ClassDeclarationSyntax classDeclare,
            CancellationToken cancellationToken)
        {
            var newDeclare = classDeclare.WithIdentifier(
                SyntaxFactory.Identifier(classDeclare.Identifier.ValueText + "EventSource"));

            var oldNode = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var newNode = oldNode.ReplaceNode(classDeclare, newDeclare);

            return document.WithSyntaxRoot(newNode!);
        }
    }
}
