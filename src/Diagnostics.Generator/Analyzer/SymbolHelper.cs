using Microsoft.CodeAnalysis;

namespace Diagnostics.Generator.Analyzer
{
    internal static class SymbolHelper
    {
        public static INamedTypeSymbol GetStringSymbol(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.String")!;
        }
        public static INamedTypeSymbol GetEventAttributeSymbol(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Diagnostics.Tracing.EventAttribute")!;
        }
        public static INamedTypeSymbol GetEventSourceSymbol(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.Diagnostics.Tracing.EventSource")!;
        }
    }
}
