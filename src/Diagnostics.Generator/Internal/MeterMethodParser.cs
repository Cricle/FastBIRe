using Diagnostics.Generator.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Diagnostics.Generator.Internal
{
    internal class MeterMethodParser : ParserBase
    {
        public void Execute(SourceProductionContext context, GeneratorTransformResult<ISymbol> node)
        {
            var symbol = (INamedTypeSymbol)node.Value;
            var methods = symbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(x => x.HasAttribute(Consts.MeterRecordAttribute.FullName))
                .ToList();
            if (methods.Count == 0)
            {
                return;
            }
            var nullableEnable = symbol.GetNullableContext(node.SemanticModel);
            var classHasUnsafe = HasKeyword(symbol, SyntaxKind.UnsafeKeyword);
            var unsafeKeyword = classHasUnsafe ? "unsafe" : string.Empty;
            var visibility = GetVisiblity(symbol);
            var staticKeyword = symbol.IsStatic ? "static" : string.Empty;
            GeneratorTransformResult<ISymbol>.GetWriteNameSpace(symbol, out var nameSpaceStart, out var nameSpaceEnd);

            var bodyBuilder = new StringBuilder();
            foreach (var item in methods)
            {
                if (!TryWriteMeterCodes(item, out var methodCode, out var diagnostic))
                {
                    if (diagnostic != null)
                    {
                        context.ReportDiagnostic(diagnostic);
                        return;
                    }
                    continue;
                }
                bodyBuilder.AppendLine(methodCode);
            }
            var code = @$"
#pragma warning disable CS8604
#nullable enable
            {nameSpaceStart}

                {visibility} {staticKeyword} {unsafeKeyword} partial class {symbol.Name}
                {{ 
                    {bodyBuilder}
                }}
            }}
#nullable restore
#pragma warning restore
                
            ";

            code = Helpers.FormatCode(code);
            context.AddSource($"{node.Value.Name}.Meters.g.cs", code);
        }

        private static ISymbol? GetTypeParamterSymbol(ISymbol symbol,int index)
        {
            if (symbol is IFieldSymbol fieldSymbol)
            {
                return ((INamedTypeSymbol)fieldSymbol.Type).TypeArguments[index];
            }
            else if (symbol is IPropertySymbol propertySymbol)
            {
                return ((INamedTypeSymbol)propertySymbol.Type).TypeArguments[index];
            }
            return null;
        }

        private static bool TryWriteMeterCodes(IMethodSymbol method, out string? code, out Diagnostic? diagnostic)
        {
            code = null;
            diagnostic = null;
            var attr = method.GetAttribute(Consts.MeterRecordAttribute.FullName)!;
            var meterMemberName = attr.GetByIndex<string>(0);
            var type = method.ContainingType.GetMembers()
                .FirstOrDefault(x => x.Name == meterMemberName);

            if (type == null)
            {
                diagnostic = Diagnostic.Create(Messages.MeterNotFound, method.Locations[0], meterMemberName, method.ContainingType.ToString());
                return false;
            }
            if (!method.ReturnsVoid || method.IsGenericMethod)
            {
                diagnostic = Diagnostic.Create(Messages.MeterMethodError, method.Locations[0]);
                return false;
            }
            //Check type
            var meterTypes = ParseMeterTypes(type);
            if (meterTypes != MeterTypes.Counter &&
                meterTypes != MeterTypes.Histogram &&
                meterTypes != MeterTypes.UpDownCounter)
            {
                diagnostic = Diagnostic.Create(Messages.UnknowMeter, method.Locations[0], type.ToString());
                return false;
            }
            //Check arg and counter type
            var meterType = GetTypeParamterSymbol(type, 0);
            if (method.Parameters.Length == 0 ||
                !SymbolEqualityComparer.Default.Equals(meterType, method.Parameters[0].Type))
            {
                diagnostic = Diagnostic.Create(Messages.MeterMethodInputError, method.Locations[0], meterType);
                return false;
            }
            var methodVisibility = GetVisiblity(method);
            var methodHead = $"{methodVisibility} {(method.IsStatic ? "static" : string.Empty)} partial void {method.Name}({string.Join(", ", method.Parameters.Select(x => x.ToString()))})";
            var thisCode = type.IsStatic ? "global::"+method.ContainingType.ToString():"this";
            var recordMethod = GetRecordMethod(meterTypes.Value);
            var codeBuilder = new StringBuilder(methodHead);
            codeBuilder.AppendLine("{");
            if (method.Parameters.Length == 1)
            {
                codeBuilder.AppendLine($"{thisCode}.{meterMemberName}.{recordMethod}({method.Parameters[0].Name});");
            }
            else if (method.Parameters.Length <= 4)
            {
                var runHead = $"{thisCode}.{meterMemberName}.{recordMethod}({method.Parameters[0].Name}";
                if (method.Parameters.Length >= 2)
                {
                    runHead += $",{CreateTagCreate(method.Parameters[1].Name)}";
                }
                if (method.Parameters.Length >= 3)
                {
                    runHead += $",{CreateTagCreate(method.Parameters[2].Name)}";
                }
                if (method.Parameters.Length >= 4)
                {
                    runHead += $",{CreateTagCreate(method.Parameters[3].Name)}";
                }
                runHead += ");";
                codeBuilder.Append(runHead);
            }
            else
            {
                codeBuilder.AppendLine("global::System.Diagnostics.TagList tagList = new global::System.Diagnostics.TagList();");
                foreach (var item in method.Parameters.Skip(1))
                {
                    codeBuilder.AppendLine($"tagList.Add(nameof({item.Name}),{item.Name});");
                }
                codeBuilder.AppendLine($"{thisCode}.{meterMemberName}.{recordMethod}({method.Parameters[0].Name},in tagList);");
            }
            if (!method.IsStatic)
            {
                codeBuilder.AppendLine($"On{method.Name}({string.Join(", ", method.Parameters.Select(x => x.Name))});");
            }
            codeBuilder.AppendLine("}");
            if (!method.IsStatic)
            {
                codeBuilder.AppendLine($"partial void On{method.Name}({string.Join(", ", method.Parameters.Select(x => x.ToString()))});");
            }
            code = codeBuilder.ToString();
            return true;
        }
        private static string CreateTagCreate(string parName)
        {
            return $"new global::System.Collections.Generic.KeyValuePair<global::System.String,global::System.Object?>(nameof({parName}),{parName})";
        }
        private static string GetRecordMethod(MeterTypes type)
        {
            switch (type)
            {
                case MeterTypes.UpDownCounter:
                case MeterTypes.Counter:
                    return "Add";
                case MeterTypes.Histogram:
                    return "Record";
                default:
                    return string.Empty;
            }
        }
        private static MeterTypes? ParseMeterTypes(ISymbol symbol)
        {
            string symboFullName = string.Empty;
            if (symbol is IFieldSymbol fieldSymbol)
            {
                symboFullName=fieldSymbol.Type.ToString().TrimEnd('?');
            }
            else if (symbol is IPropertySymbol methodSymbol)
            {
                symboFullName = methodSymbol.Type.ToString().TrimEnd('?');
            }
            if (symboFullName.StartsWith("System.Diagnostics.Metrics.Counter"))
            {
                return MeterTypes.Counter;
            }
            if (symboFullName.StartsWith("System.Diagnostics.Metrics.Histogram"))
            {
                return MeterTypes.Histogram;
            }
            if (symboFullName.StartsWith("System.Diagnostics.Metrics.ObservableGauge"))
            {
                return MeterTypes.ObservableGauge;
            }
            if (symboFullName.StartsWith("System.Diagnostics.Metrics.ObservableUpDownCounter"))
            {
                return MeterTypes.ObservableUpDownCounter;
            }
            if (symboFullName.StartsWith("System.Diagnostics.Metrics.UpDownCounter"))
            {
                return MeterTypes.UpDownCounter;
            }
            return null;
        }


        public static bool Predicate(SyntaxNode node, CancellationToken token)
        {
            return true;
        }
        public static GeneratorTransformResult<ISymbol> Transform(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            return new GeneratorTransformResult<ISymbol>(context.TargetSymbol, context);
        }
    }
}
