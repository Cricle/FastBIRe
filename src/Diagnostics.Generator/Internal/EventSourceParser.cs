﻿using Diagnostics.Generator.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Diagnostics.Generator.Internal
{
    internal class EventSourceParser
    {
        public void Execute(SourceProductionContext context, GeneratorTransformResult<ISymbol> node)
        {
            var symbol = (INamedTypeSymbol)node.SyntaxContext.TargetSymbol;
            var nullableEnable = symbol.GetNullableContext(node.SemanticModel);
            var visibility = GetVisiblity(symbol);
            node.GetWriteNameSpace(out var nameSpaceStart, out var nameSpaceEnd);

            var fullName = node.GetTypeFullName();
            var className = symbol.Name;
            var @namespace = node.GetNameSpace();
            if (!string.IsNullOrEmpty(@namespace))
            {
                @namespace = "global::" + @namespace;
            }
            var nullableEnd = symbol.IsReferenceType && (nullableEnable & NullableContext.Enabled) != 0 ? "?" : string.Empty;

            var ctxNullableEnd = (nullableEnable & NullableContext.Enabled) != 0 ? "?" : string.Empty;

            var methods = symbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(x => x.HasAttribute(Consts.EventAttribute.Name) && (x.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is MethodDeclarationSyntax m &&
                    m.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword))))
                .ToList();
            var command = GenerateOnEventCommand(symbol, ctxNullableEnd, out var diagnostic);
            if (diagnostic != null)
            {
                context.ReportDiagnostic(diagnostic);
                return;
            }
            var methodBody = new StringBuilder();
            foreach (var item in methods)
            {
                var str = GenerateMethod(item, out var diag);
                if (diag != null)
                {
                    context.ReportDiagnostic(diag);
                    return;
                }
                methodBody.AppendLine(str);
            }
            var code = @$"
#nullable enable
            {nameSpaceStart}

                {Consts.CompilerGenerated}
                {visibility} partial class {className}
                {{
                    {methodBody}

                    {command}
                    [global::System.Diagnostics.Tracing.NonEventAttribute]
                    partial void OnEventCommandExecuted(global::System.Diagnostics.Tracing.EventCommandEventArgs command);
                }}
            }}
#nullable restore
                ";

            code = Helpers.FormatCode(code);
            context.AddSource($"{className}.g.cs", code);
        }
        private bool IsSupportCounterType(ITypeSymbol type)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                    return true;
                default:
                    return false;
            }
        }
        private bool IsCounterType(ITypeSymbol type)
        {
            switch (type.ToString().TrimEnd('?'))
            {
                case "System.Diagnostics.Tracing.IncrementingEventCounter":
                case "System.Diagnostics.Tracing.EventCounter":
                case "System.Diagnostics.Tracing.PollingCounter":
                case "System.Diagnostics.Tracing.IncrementingPollingCounter":
                    return true;
                default:
                    return false;
            }
        }
        private bool IsEventCounterType(ITypeSymbol type)
        {
            switch (type.ToString().TrimEnd('?'))
            {
                case "System.Diagnostics.Tracing.IncrementingEventCounter":
                case "System.Diagnostics.Tracing.EventCounter":
                    return true;
                default:
                    return false;
            }
        }
        private string GetCounterName(CounterTypes type)
        {
            switch (type)
            {
                case CounterTypes.EventCounter:
                    return "global::System.Diagnostics.Tracing.EventCounter";
                case CounterTypes.IncrementingEventCounter:
                    return "global::System.Diagnostics.Tracing.IncrementingEventCounter";
                case CounterTypes.PollingCounter:
                    return "global::System.Diagnostics.Tracing.PollingCounter";
                case CounterTypes.IncrementingPollingCounter:
                    return "global::System.Diagnostics.Tracing.IncrementingPollingCounter";
                default:
                    throw new NotSupportedException(type.ToString());
            }
        }
        private string GenerateCreateCounter(string left, string name, CounterTypes type, double displayRateTimeScaleMs, string? displayUnits, string? displayName, string? right)
        {
            var typeName = GetCounterName(type);
            switch (type)
            {
                case CounterTypes.EventCounter:
                    return $"{left} new {typeName}(\"{name}\",this){{ DisplayName = \"{displayName}\",DisplayUnits=\"{displayUnits}\" }}";
                case CounterTypes.IncrementingEventCounter:
                    return $"{left} new {typeName}(\"{name}\",this){{ DisplayName = \"{displayName}\",DisplayUnits=\"{displayUnits}\", DisplayRateTimeScale=global::System.TimeSpan.FromMilliseconds({displayRateTimeScaleMs})}}";
                case CounterTypes.PollingCounter:
                    return $"{left} new {typeName}(\"{name}\",this,{right}){{ DisplayName = \"{displayName}\",DisplayUnits=\"{displayUnits}\" }}";
                case CounterTypes.IncrementingPollingCounter:
                    return $"{left} new {typeName}(\"{name}\",this{right}){{ DisplayName = \"{displayName}\",DisplayUnits=\"{displayUnits}\", DisplayRateTimeScale=global::System.TimeSpan.FromMilliseconds({displayRateTimeScaleMs})}}";
                default:
                    throw new NotSupportedException(type.ToString());
            }
        }
        private string GenerateOnEventCommand(INamedTypeSymbol symbol,string nullableTail, out Diagnostic? diagnostic)
        {
            var counter = symbol.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(x => x.HasAttribute(Consts.CounterAttibute.Name))
                .ToList();
            var addins = new StringBuilder();
            var bodys = new StringBuilder();
            foreach (var item in counter)
            {
                var counterAttr = item.GetAttribute(Consts.CounterAttibute.Name)!;
                var name = counterAttr.GetByIndex<string>(0)!;
                var type = counterAttr.GetByIndex<CounterTypes>(1);
                var displayRateTimeScaleMs = counterAttr.GetByNamed<double>(Consts.CounterAttibute.DisplayRateTimeScaleMs);
                var displayUnits = counterAttr.GetByNamed<string>(Consts.CounterAttibute.DisplayUnits);
                var displayName = counterAttr.GetByNamed<string>(Consts.CounterAttibute.DisplayName);

                var typeName = GetCounterName(type);
                var isSupportCounterType = IsSupportCounterType(item.Type);
                var isEventCouterType = IsEventCounterType(item.Type);
                var isPollingCounter = type == CounterTypes.PollingCounter || type == CounterTypes.IncrementingPollingCounter;
                var isEventCounter = type == CounterTypes.EventCounter || type == CounterTypes.IncrementingEventCounter;

                if (!isSupportCounterType && !IsCounterType(item.Type))
                {
                    diagnostic = Diagnostic.Create(Messages.FieldMustReturnNumber, item.Locations[0]);
                    return string.Empty;
                }
                if (!isPollingCounter && !isEventCouterType)
                {
                    diagnostic = Diagnostic.Create(Messages.PollingCounterMustSimpleType, item.Locations[0]);
                    return string.Empty;
                }
                if ((type== CounterTypes.IncrementingPollingCounter||type== CounterTypes.IncrementingEventCounter) && displayRateTimeScaleMs <= 0)
                {
                    diagnostic = Diagnostic.Create(Messages.PollingCounterMustInputRate, item.Locations[0]);
                    return string.Empty;
                }
                if (isEventCounter && !isEventCouterType)
                {
                    diagnostic = Diagnostic.Create(Messages.EventCounterMustType, item.Locations[0]);
                    return string.Empty;
                }
                if (isEventCouterType)
                {
                    bodys.AppendLine(GenerateCreateCounter($"{item.Name} ??=", name, type, displayRateTimeScaleMs, displayUnits, displayName, null) + ";");
                }
                else
                {
                    var counterName = item.Name + "Counter";
                    addins.AppendFormat("private {0}{1} {2}; \n", typeName, nullableTail, counterName);
                    var specialName = item.Name.TrimStart('_');
                    specialName = char.ToUpper(specialName[0]) + specialName.Substring(1);
                    addins.AppendLine($@"
[global::System.Diagnostics.Tracing.NonEventAttribute]
public void Increment{specialName}(global::System.Int32 inc=1)
{{
    if(inc==1)
        global::System.Threading.Interlocked.Increment(ref {item.Name});
    else
        global::System.Threading.Interlocked.Add(ref {item.Name},inc);
}}
");
                    bodys.AppendLine(GenerateCreateCounter($"{counterName} ??=", name, type, displayRateTimeScaleMs, displayUnits, displayName, $"()=>global::System.Threading.Interlocked.Read(ref {item.Name})") + ";");
                }
            }

            var code = $@"
#region Counter generated 
{addins}
protected override void OnEventCommand(global::System.Diagnostics.Tracing.EventCommandEventArgs command)
{{
    if (command.Command == global::System.Diagnostics.Tracing.EventCommand.Enable)
    {{
        {bodys}
    }}
    OnEventCommandExecuted(command);
}}
#endregion
";
            diagnostic = null;
            return code;
        }
        private static readonly HashSet<SpecialType> supportTypes = new HashSet<SpecialType>
        {
             SpecialType.System_Boolean,
             SpecialType.System_Byte,
             SpecialType.System_SByte,
             SpecialType.System_Char,
             SpecialType.System_Int16,
             SpecialType.System_UInt16,
             SpecialType.System_Int32,
             SpecialType.System_Int64,
             SpecialType.System_UInt64,
             SpecialType.System_UInt32,
             SpecialType.System_Single,
             SpecialType.System_Double,
             SpecialType.System_Decimal,
             SpecialType.System_String,
             SpecialType.System_DateTime
        };
        private bool IsSupportType(ITypeSymbol type)
        {
            if (supportTypes.Contains(type.SpecialType))
            {
                return true;
            }
            if (type.IsValueType)
            {
                switch (type.ToString())
                {
                    case "System.Guid":
                    case "System.TimeSpan":
                    case "System.DateTimeOffset":
                    case "System.UIntPtr":
                        return true;
                    default:
                        break;
                }
                if (type.OriginalDefinition != null &&
                    type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T &&
                    type is INamedTypeSymbol symbol &&
                    symbol.TypeArguments.Length == 1)
                {
                    return IsSupportType(symbol.TypeArguments[0]);
                }
            }
            return false;
        }
        private string GenerateMethod(IMethodSymbol method, out Diagnostic? diagnostic)
        {
            var pars = method.Parameters;
            for (int i = 0; i < pars.Length; i++)
            {
                var par = pars[i];
                var isSupportType = IsSupportType(par.Type);
                if (!isSupportType)
                {
                    diagnostic = Diagnostic.Create(Messages.NotSupportType, par.Locations[0]);
                    return string.Empty;
                }
            }
            //relatedActivityId
            var relatedActivityIds = pars.Where(x => x.HasAttribute(Consts.RelatedActivityIdAttribute.Name)).ToList();
            if (relatedActivityIds.Count > 1)
            {
                diagnostic = Diagnostic.Create(Messages.RelatedActivityIdOnlyOne, relatedActivityIds[1].Locations[0]);
                return string.Empty;
            }
            var relate = "null";
            if (relatedActivityIds.Count != 0)
            {
                if (relatedActivityIds[0].Type.ToString() != "System.Guid")
                {
                    diagnostic = Diagnostic.Create(Messages.RelatedActivityIdMustGuid, relatedActivityIds[0].Locations[0]);
                    return string.Empty;
                }
                relate = $"&{relatedActivityIds[0].Name}";
            }
            var unsafeKeyWord = "unsafe";
            var actualDatasCount = pars.Length - relatedActivityIds.Count;
            var datasDeclare = $"global::System.Diagnostics.Tracing.EventSource.EventData* datas = stackalloc global::System.Diagnostics.Tracing.EventSource.EventData[{actualDatasCount}];";
            var datasPar = "datas";
            if (actualDatasCount == 0)
            {
                unsafeKeyWord = datasDeclare = string.Empty;
                datasPar = "null";
            }

            var eventId = method.GetAttribute(Consts.EventAttribute.Name)!
                .GetByIndex<int>(0);
            var invokeMethod = $"WriteEventWithRelatedActivityIdCore({eventId},{relate}, {actualDatasCount}, {datasPar});";
            if (actualDatasCount == 0)
            {
                invokeMethod = $"WriteEvent({eventId});";
            }
            var s = $@"
{GeneratorTransformResult<ISymbol>.GetAccessibilityString(method.DeclaredAccessibility)} {unsafeKeyWord} partial {method.ReturnType} {method.Name}({string.Join(",", pars.Select(x => x.ToString()))})
{{
    {datasDeclare}
    {string.Join("\n", pars.Except(relatedActivityIds).Select(GenerateWrite))}
    {invokeMethod}
}}";
            diagnostic = null;
            return s;
        }
        private string GenerateWrite(IParameterSymbol parameter, int index)
        {
            if (parameter.Type.SpecialType == SpecialType.System_String)
            {
                return $@"
datas[{index}] = new global::System.Diagnostics.Tracing.EventSource.EventData
{{
    DataPointer = {parameter.Name}==null?global::System.IntPtr.Zero:(nint)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::System.Runtime.InteropServices.MemoryMarshal.GetReference({parameter.Name}.AsSpan())),
    Size ={parameter.Name} == null ? 0 : checked(({parameter.Name}.Length + 1) * sizeof(char))
}};";
            }
            return $@"
datas[{index}] = new global::System.Diagnostics.Tracing.EventSource.EventData
{{
    DataPointer = (nint)(&{parameter.Name}),
    Size = sizeof({parameter.Type})
}};
";
        }
        public string GetVisiblity(ISymbol symbol)
        {
            return GeneratorTransformResult<ISymbol>.GetAccessibilityString(symbol.DeclaredAccessibility);
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
