using Microsoft.CodeAnalysis;
using System.Linq;

namespace FastBIRe.ModelGen.Internal
{
    internal class GeneratorTransformResult<T>
    {
        public GeneratorTransformResult(T value, GeneratorAttributeSyntaxContext syntaxContext)
        {
            Value = value;
            SyntaxContext = syntaxContext;
        }

        public T Value { get; }

        public SemanticModel SemanticModel => SyntaxContext.SemanticModel;

        public GeneratorAttributeSyntaxContext SyntaxContext { get; }

        public IAssemblySymbol AssemblySymbol => SyntaxContext.SemanticModel.Compilation.Assembly;

        public bool IsAutoGen()
        {
            return IsAutoGen(SyntaxContext.TargetSymbol);
        }
        public string GetNameSpace()
        {
            return GetNameSpace(SyntaxContext.TargetSymbol);
        }
        public string GetTypeFullName() 
        {
            return GetTypeFullName(SyntaxContext.TargetSymbol);
        }
        public void GetWriteNameSpace(out string nameSpaceStart, out string nameSpaceEnd)
        {
            GetWriteNameSpace(SyntaxContext.TargetSymbol, out nameSpaceStart, out nameSpaceEnd);
        }
        public string GetAccessibilityString()
        {
            return GetAccessibilityString(SyntaxContext.TargetSymbol.DeclaredAccessibility);
        }

        public static bool IsAutoGen(ISymbol symbol)
        {
            return symbol.GetAttributes()
                    .Any(x => x.AttributeClass?.ToString() == typeof(GeneratorAttribute).FullName);
        }
        public const string GlobalNs = "<global namespace>";
        public const string GlobalNsKeyword = "<global";

        public static string GetTypeFullName(ISymbol symbol)
        {
            var rawNameSpace = GetNameSpace(symbol);
            if (rawNameSpace.Contains(GlobalNsKeyword))
            {
                return string.Empty;
            }
            return $"global::{symbol}";
        }
        public static void GetWriteNameSpace(ISymbol symbol, out string nameSpaceStart, out string nameSpaceEnd)
        {
            var rawNameSpace = GetNameSpace(symbol);
            nameSpaceStart = $"namespace {rawNameSpace}\n{{";
            nameSpaceEnd = "}";
            if (string.IsNullOrEmpty(rawNameSpace))
            {
                nameSpaceStart = string.Empty;
                nameSpaceEnd = string.Empty;
            }
        }
        public static string GetNameSpace(ISymbol symbol)
        {
            var ns = symbol.ContainingNamespace.ToString();
            if (ns.Contains(GlobalNsKeyword))
            {
                return string.Empty;
            }
            return ns;
        }
        public static string GetAccessibilityString(Accessibility accessibility)
        {
            if (accessibility == Accessibility.Private)
            {
                return "private";
            }
            if (accessibility == Accessibility.ProtectedAndInternal)
            {
                return "protected internal";
            }
            if (accessibility == Accessibility.Protected)
            {
                return "protected";
            }
            if (accessibility == Accessibility.Internal)
            {
                return "internal";
            }
            if (accessibility == Accessibility.Public)
            {
                return "public";
            }
            return string.Empty;
        }
    }
}
