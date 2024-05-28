using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace FastBIRe.ModelGen.Internal
{
    internal static class Helpers
    {
        public static string FormatCode(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();
            return root.NormalizeWhitespace().ToFullString();
        }
    }
}
