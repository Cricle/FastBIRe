using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Diagnostics.Generator.CodeFixes
{
    internal static class WriteEventBodyHelper
    {
        private static readonly TypeSyntax eventDataName = SyntaxFactory.QualifiedName(
        SyntaxFactory.QualifiedName(
            SyntaxFactory.QualifiedName(
                SyntaxFactory.QualifiedName(
                    SyntaxFactory.AliasQualifiedName(
                        SyntaxFactory.IdentifierName(
                            SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                        SyntaxFactory.IdentifierName("System")),
                    SyntaxFactory.IdentifierName("Diagnostics")),
                SyntaxFactory.IdentifierName("Tracing")),
            SyntaxFactory.IdentifierName("EventSource")),
        SyntaxFactory.IdentifierName("EventData"));
        public static StatementSyntax GenerateDeclare(string name, int size)
        {
            return SyntaxFactory.LocalDeclarationStatement(
    SyntaxFactory.VariableDeclaration(
        SyntaxFactory.PointerType(eventDataName))
    .WithVariables(
        SyntaxFactory.SingletonSeparatedList(
            SyntaxFactory.VariableDeclarator(
                SyntaxFactory.Identifier(name))
            .WithInitializer(
                SyntaxFactory.EqualsValueClause(
                    SyntaxFactory.StackAllocArrayCreationExpression(
                        SyntaxFactory.ArrayType(eventDataName)
                        .WithRankSpecifiers(
                            SyntaxFactory.SingletonList(
                                SyntaxFactory.ArrayRankSpecifier(
                                    SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.NumericLiteralExpression,
                                            SyntaxFactory.Literal(size))))))))))));
        }
        public static StatementSyntax GenerateStructAssign(string varName, string name, int index)
        {
            return SyntaxFactory.ExpressionStatement(
     SyntaxFactory.AssignmentExpression(
         SyntaxKind.SimpleAssignmentExpression,
         SyntaxFactory.ElementAccessExpression(
             SyntaxFactory.IdentifierName(varName))
         .WithArgumentList(
             SyntaxFactory.BracketedArgumentList(
                 SyntaxFactory.SingletonSeparatedList(
                     SyntaxFactory.Argument(
                         SyntaxFactory.LiteralExpression(
                             SyntaxKind.NumericLiteralExpression,
                             SyntaxFactory.Literal(index)))))),
         SyntaxFactory.ObjectCreationExpression(eventDataName)
             .WithInitializer(
                 SyntaxFactory.InitializerExpression(
                     SyntaxKind.ObjectInitializerExpression,
                     SyntaxFactory.SeparatedList<ExpressionSyntax>(
                         new SyntaxNodeOrToken[]{
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName("DataPointer"),
                                SyntaxFactory.CastExpression(
                                    SyntaxFactory.IdentifierName("nint"),
                                    SyntaxFactory.ParenthesizedExpression(
                                        SyntaxFactory.PrefixUnaryExpression(
                                            SyntaxKind.AddressOfExpression,
                                            SyntaxFactory.IdentifierName(name))))),
                            SyntaxFactory.Token(SyntaxKind.CommaToken),
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName("Size"),
                                SyntaxFactory.SizeOfExpression(
                                    SyntaxFactory.PredefinedType(
                                        SyntaxFactory.Token(SyntaxKind.IntKeyword))))})))));
        }
        public static StatementSyntax GenerateStringAssign(string varName, string name, int index)
        {
            return SyntaxFactory.ExpressionStatement(
    SyntaxFactory.AssignmentExpression(
        SyntaxKind.SimpleAssignmentExpression,
        SyntaxFactory.ElementAccessExpression(
            SyntaxFactory.IdentifierName(varName))
        .WithArgumentList(
            SyntaxFactory.BracketedArgumentList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Argument(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            SyntaxFactory.Literal(index)))))),
        SyntaxFactory.ObjectCreationExpression(eventDataName)
        .WithInitializer(
            SyntaxFactory.InitializerExpression(
                SyntaxKind.ObjectInitializerExpression,
                SyntaxFactory.SeparatedList<ExpressionSyntax>(
                    new SyntaxNodeOrToken[]{
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName("DataPointer"),
                            SyntaxFactory.ConditionalExpression(
                                SyntaxFactory.BinaryExpression(
                                    SyntaxKind.EqualsExpression,
                                    SyntaxFactory.IdentifierName(name),
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.NullLiteralExpression)),
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.AliasQualifiedName(
                                            SyntaxFactory.IdentifierName(
                                                SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                                            SyntaxFactory.IdentifierName("System")),
                                        SyntaxFactory.IdentifierName("IntPtr")),
                                    SyntaxFactory.IdentifierName("Zero")),
                                SyntaxFactory.CastExpression(
                                    SyntaxFactory.IdentifierName("nint"),
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        SyntaxFactory.AliasQualifiedName(
                                                            SyntaxFactory.IdentifierName(
                                                                SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                                                            SyntaxFactory.IdentifierName("System")),
                                                        SyntaxFactory.IdentifierName("Runtime")),
                                                    SyntaxFactory.IdentifierName("CompilerServices")),
                                                SyntaxFactory.IdentifierName("Unsafe")),
                                            SyntaxFactory.IdentifierName("AsPointer")))
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SingletonSeparatedList(
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.InvocationExpression(
                                                        SyntaxFactory.MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            SyntaxFactory.MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                SyntaxFactory.MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    SyntaxFactory.MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        SyntaxFactory.AliasQualifiedName(
                                                                            SyntaxFactory.IdentifierName(
                                                                                SyntaxFactory.Token(SyntaxKind.GlobalKeyword)),
                                                                            SyntaxFactory.IdentifierName("System")),
                                                                        SyntaxFactory.IdentifierName("Runtime")),
                                                                    SyntaxFactory.IdentifierName("InteropServices")),
                                                                SyntaxFactory.IdentifierName("MemoryMarshal")),
                                                            SyntaxFactory.IdentifierName("GetReference")))
                                                    .WithArgumentList(
                                                        SyntaxFactory.ArgumentList(
                                                            arguments: SyntaxFactory.SingletonSeparatedList(
                                                                SyntaxFactory.Argument(
                                                                    SyntaxFactory.InvocationExpression(
                                                                        SyntaxFactory.MemberAccessExpression(
                                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                                            SyntaxFactory.IdentifierName(name),
                                                                            SyntaxFactory.IdentifierName("AsSpan"))))))))
                                                .WithRefOrOutKeyword(
                                                    SyntaxFactory.Token(SyntaxKind.RefKeyword)))))))),
                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName("Size"),
                            SyntaxFactory.ConditionalExpression(
                                SyntaxFactory.BinaryExpression(
                                    SyntaxKind.EqualsExpression,
                                    SyntaxFactory.IdentifierName(name),
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.NullLiteralExpression)),
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    SyntaxFactory.Literal(0)),
                                SyntaxFactory.CheckedExpression(
                                    SyntaxKind.CheckedExpression,
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.MultiplyExpression,
                                        SyntaxFactory.ParenthesizedExpression(
                                            SyntaxFactory.BinaryExpression(
                                                SyntaxKind.AddExpression,
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.IdentifierName(name),
                                                    SyntaxFactory.IdentifierName("Length")),
                                                SyntaxFactory.LiteralExpression(
                                                    SyntaxKind.NumericLiteralExpression,
                                                    SyntaxFactory.Literal(1)))),
                                        SyntaxFactory.SizeOfExpression(
                                            SyntaxFactory.PredefinedType(
                                                SyntaxFactory.Token(SyntaxKind.CharKeyword)))))))})))));
        }
        public static StatementSyntax GenerateWriteEvent(string varName, int eventId, int dataCount)
        {
            return SyntaxFactory.ExpressionStatement(
    SyntaxFactory.InvocationExpression(
        SyntaxFactory.IdentifierName("WriteEventCore"))
    .WithArgumentList(
        SyntaxFactory.ArgumentList(
            SyntaxFactory.SeparatedList<ArgumentSyntax>(
                new SyntaxNodeOrToken[]{
                    SyntaxFactory.Argument(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            SyntaxFactory.Literal(eventId))),
                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                    SyntaxFactory.Argument(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            SyntaxFactory.Literal(dataCount))),
                    SyntaxFactory.Token(SyntaxKind.CommaToken),
                    SyntaxFactory.Argument(
                        SyntaxFactory.IdentifierName(varName))}))));
        }

        public static StatementSyntax GenerateWriteEvent(int eventId)
        {
            return SyntaxFactory.ExpressionStatement(
     SyntaxFactory.InvocationExpression(
         SyntaxFactory.IdentifierName("WriteEvent"))
     .WithArgumentList(
         SyntaxFactory.ArgumentList(
             SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                 SyntaxFactory.Argument(
                     SyntaxFactory.LiteralExpression(
                         SyntaxKind.NumericLiteralExpression,
                         SyntaxFactory.Literal(eventId)))))));
        }

    }
}
