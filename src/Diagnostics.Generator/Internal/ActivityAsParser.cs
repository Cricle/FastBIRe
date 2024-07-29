using Diagnostics.Generator.Core;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Diagnostics.Generator.Internal
{
    internal class ActivityAsParser : ParserBase
    {
        private static List<ISymbol> GetProvidedSymbols(INamedTypeSymbol symbol)
        {
            return symbol.GetMembers()
                .Where(x => !x.IsStatic)
                .Where(x => x is not IPropertySymbol propertySymbol || !propertySymbol.IsIndexer)
                .Where(x => x.DeclaredAccessibility == Accessibility.Public || x.DeclaredAccessibility == Accessibility.Internal || x.DeclaredAccessibility == Accessibility.ProtectedOrInternal)
                .Where(x => x is IPropertySymbol || x is IFieldSymbol)
                .Where(x => !x.HasAttribute(Consts.ActivityAsIgnoreAttribute.FullName))
                .ToList();
        }

        public void Execute(SourceProductionContext context, GeneratorTransformResult<ISymbol> node)
        {
            var symbol = (INamedTypeSymbol)node.Value;
            var origin = symbol;
            GeneratorTransformResult<ISymbol>.GetWriteNameSpace(symbol, node.SemanticModel, out var nameSpaceStart, out var nameSpaceEnd);
            var attr = symbol.GetAttribute(Consts.ActivityAsAttribute.FullName)!;
            var target = attr.GetByNamed<ISymbol>(Consts.ActivityAsAttribute.TargetType);
            var isSelf = true;
            if (target != null)
            {
                if (target is not INamedTypeSymbol)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Messages.TagAsTargetNotClassOrStructError, symbol.Locations[0], target.ToString()));
                    return;
                }
                symbol = (INamedTypeSymbol)target;
                isSelf = false;
            }
            else if (origin.IsStatic)
            {
                context.ReportDiagnostic(Diagnostic.Create(Messages.TagAsSelfMustNotStaticError, symbol.Locations[0]));
                return;
            }
            var props = GetProvidedSymbols(symbol);
            if (props.Count == 0)
            {
                return;
            }
            var visibility = GetVisiblity(origin);
            var asMethod = attr.GetByNamed<ActivityAsTypes>(Consts.ActivityAsAttribute.As) == ActivityAsTypes.Tag ? "SetTag" : "SetBaggage";
            var kindKeyword = origin.TypeKind == TypeKind.Class ? "class" : "struct";
            var symbolFullName = GeneratorTransformResult<ISymbol>.GetTypeFullName(symbol);
            var interfaces = string.Empty;
            if (!origin.IsStatic)
            {
                interfaces = $":global::Diagnostics.Generator.Core.IActivityTagWriter<{symbolFullName}>,global::Diagnostics.Generator.Core.IActivityTagWriter,global::Diagnostics.Generator.Core.IActivityTagMerge<{symbolFullName}>,global::Diagnostics.Generator.Core.IActivityTagMerge";
                if (isSelf)
                {
                    interfaces += ",global::Diagnostics.Generator.Core.IActivityTagExporter";
                }
            }
            var bodyBuilder = new StringBuilder();
            var writeBuilder = new StringBuilder();
            var makeTagBuilder = new StringBuilder();
            asMethod = $"activity.{asMethod}";
            var additionPrefx = attr.GetByNamed<string>(Consts.ActivityAsAttribute.Key) ?? string.Empty;
            var generateSingleton = attr.GetByNamed<bool>(Consts.ActivityAsAttribute.GenerateSingleton);
            var ignorePaths = new HashSet<string>(attr.GetByNamedArray<string>(Consts.ActivityAsAttribute.IgnorePaths) ?? Array.Empty<string>());
            if (!string.IsNullOrEmpty(additionPrefx))
            {
                additionPrefx += ".";
            }
            //Debugger.Launch();
            var deepType = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
            var deepTypeTag = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
            foreach (var item in props)
            {
                WriteProps(writeBuilder, origin, additionPrefx, "input.", string.Empty, item, asMethod, false, deepType, ignorePaths, out var diagnostic);
                if (diagnostic != null)
                {
                    context.ReportDiagnostic(diagnostic);
                    return;
                }
                WriteProps(makeTagBuilder, origin, additionPrefx, "input.", string.Empty, item, "tags", true, deepTypeTag, ignorePaths, out diagnostic);
                if (diagnostic != null)
                {
                    context.ReportDiagnostic(diagnostic);
                    return;
                }
            }
            var nullCheck = string.Empty;
            var staticKeywork = origin.IsStatic ? "static" : string.Empty;
            if (IsNullableType(symbol))
            {
                nullCheck = $@"
if(input==null)
{{
    return;
}}";
                symbolFullName += "?";
            }
            bodyBuilder.AppendLine($@"
public {staticKeywork} void Write(global::System.Diagnostics.Activity? activity, {symbolFullName} input)
{{
    {nullCheck}

    if(activity==null)
    {{
        return;
    }}

    {writeBuilder}
}}

public {staticKeywork} void Merge(global::System.Diagnostics.ActivityTagsCollection tags,{symbolFullName} input)
{{
    if(tags==null)
    {{
        return;
    }}

    {nullCheck}

    {makeTagBuilder}
}}
public {staticKeywork} global::System.Diagnostics.ActivityTagsCollection Merge({symbolFullName} input)
{{
    global::System.Diagnostics.ActivityTagsCollection tags = new global::System.Diagnostics.ActivityTagsCollection();
    Merge(tags,input);
    return tags;
}}

public {staticKeywork} void Merge(global::System.Diagnostics.ActivityTagsCollection tags, object input)
{{
    Merge(tags,({symbolFullName})input);
}}

public {staticKeywork} global::System.Diagnostics.ActivityTagsCollection Merge(object input)
{{
    return Merge(({symbolFullName})input);
}}
");
            if (!origin.IsStatic)
            {
                bodyBuilder.AppendLine($@"
void global::Diagnostics.Generator.Core.IActivityTagWriter.Write(global::System.Diagnostics.Activity? activity, global::System.Object input)
{{
    Write(activity,({symbolFullName})input);
}}
");
            }

            if (isSelf)
            {
                bodyBuilder.AppendLine($@"
public {staticKeywork} void Write(global::System.Diagnostics.Activity? activity)
{{
    Write(activity,this);
}}
");
            }
            var singletonExp = string.Empty;
            var originFullName = GeneratorTransformResult<ISymbol>.GetTypeFullName(origin);
            if (!origin.IsStatic && generateSingleton)
            {
                singletonExp = $"public static readonly {originFullName} Instance = new {originFullName}();";
            }
            var code = $@"

#nullable enable
            {nameSpaceStart}
                {visibility} {staticKeywork} partial {kindKeyword} {origin.Name} {interfaces}
                {{ 
                    {singletonExp}

                    {bodyBuilder}
                }}
            {nameSpaceEnd}
#nullable restore";

            code = Helpers.FormatCode(code);

            context.AddSource($"{origin.Name}.TagWrite.g.cs", code);
        }
        private static bool IsNullableStruct(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
            {
                if (namedTypeSymbol.IsGenericType && typeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
                {
                    return true;
                }
            }
            return false;

        }
        private static bool IsNullableType(ITypeSymbol typeSymbol)
        {
            if (typeSymbol.TypeKind == TypeKind.Class)
            {
                return true;
            }
            return IsNullableStruct(typeSymbol);
        }

        private static bool CanDeep(ITypeSymbol typeSymbol)
        {
            if (typeSymbol.SpecialType == SpecialType.None && typeSymbol is INamedTypeSymbol)
            {
                if (typeSymbol.ToString().StartsWith("System.Collections."))
                {
                    return false;
                }
                if (typeSymbol.TypeKind != TypeKind.Class)
                {
                    if (IsNullableStruct(typeSymbol))
                    {
                        return ((INamedTypeSymbol)typeSymbol).TypeArguments[0].SpecialType == SpecialType.None;
                    }
                    return typeSymbol.SpecialType == SpecialType.None;
                }
                return true;
            }
            return false;
        }
        private static ISymbol GetActualSymbol(ITypeSymbol typeSymbol)
        {
            if (IsNullableStruct(typeSymbol))
            {
                return ((INamedTypeSymbol)typeSymbol).TypeArguments[0];
            }
            return typeSymbol;
        }
        private static void WriteProps(StringBuilder builder,
            ISymbol originSymbol,
            string prefx,
            string invokePrefx,
            string path,
            ISymbol symbol,
            string call,
            bool isIndexCall,
            HashSet<ISymbol> deeps,
            HashSet<string> ignorePaths,
            out Diagnostic? diagnostic)
        {
            diagnostic = null;
            var name = symbol.Name;
            var res = symbol.GetAttribute(Consts.ActivityAsNameAttribute.FullName)?.GetByIndex<string>(0);
            if (!string.IsNullOrEmpty(res))
            {
                name = res;
            }
            name = name!.ToLower();
            ITypeSymbol? valSymbol = null;

            if (symbol is IPropertySymbol propertySymbol)
            {
                valSymbol = propertySymbol.Type;
            }
            else if (symbol is IFieldSymbol fieldSymbol)
            {
                valSymbol = fieldSymbol.Type;
            }
            if (valSymbol == null)
            {
                return;
            }
            //Debugger.Launch();
            var nullable = IsNullableType(valSymbol);
            var targetPath = symbol.Name;
            if (!string.IsNullOrEmpty(path))
            {
                targetPath = path + "." + symbol.Name;
            }
            if (ignorePaths.Contains(targetPath))
            {
                return;
            }
            if (CanDeep(valSymbol))
            {
                if (!deeps.Add(valSymbol))
                {
                    diagnostic = Diagnostic.Create(Messages.TagAsLoopReferenceError, originSymbol.Locations[0], valSymbol.ToString(), invokePrefx.TrimEnd('.')+"."+ targetPath);
                    return;
                }
                var actualSymbol = GetActualSymbol(valSymbol);
                var isNullableChanged = !SymbolEqualityComparer.Default.Equals(actualSymbol, valSymbol);
                var props = GetProvidedSymbols((INamedTypeSymbol)actualSymbol);
                if (nullable)
                {
                    builder.AppendLine($"if({invokePrefx}{symbol.Name}!=null)");
                    builder.AppendLine("{");
                }
                var propPrefx = prefx + name + ".";
                var propInvokPrefx = invokePrefx + symbol.Name + ".";
                if (isNullableChanged)
                {
                    propInvokPrefx += "Value.";
                }
                foreach (var item in props)
                {
                    WriteProps(builder, originSymbol, propPrefx, propInvokPrefx, targetPath, item, call, isIndexCall, deeps, ignorePaths, out diagnostic);
                    if (diagnostic != null)
                    {
                        return;
                    }
                }
                if (nullable)
                {
                    builder.AppendLine("}");
                }
            }
            else
            {
                var targetExp = invokePrefx + symbol.Name;
                string callExp;
                if (isIndexCall)
                {
                    callExp = $"{call}[\"{prefx}{name}\"]={targetExp};";
                }
                else
                {
                    callExp = $"{call}(\"{prefx}{name}\",{targetExp});";
                }
                if (nullable)
                {
                    callExp = $@"
if({targetExp} != null)
{{ 
    {callExp}
}}";
                }
                builder.AppendLine(callExp);
            }
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
