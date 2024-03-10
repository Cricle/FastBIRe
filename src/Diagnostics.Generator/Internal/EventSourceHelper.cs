using Diagnostics.Generator.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Diagnostics.Generator.Internal
{
    internal static class EventSourceHelper
    {
        public static string GetVisiblity(ISymbol symbol)
        {
            return GeneratorTransformResult<ISymbol>.GetAccessibilityString(symbol.DeclaredAccessibility);
        }
        public static string GetSpecialName(string name)
        {
            var specialName = name.TrimStart('_');
            return char.ToUpper(specialName[0]) + specialName.Substring(1);
        }
        public static bool TryWriteCode(SourceProductionContext context,SemanticModel model,INamedTypeSymbol symbol,bool logMode,IEnumerable<IMethodSymbol> methods,out string? outCode)
        {
            var nullableEnable = symbol.GetNullableContext(model);
            var visibility = GetVisiblity(symbol);
            GeneratorTransformResult<ISymbol>.GetWriteNameSpace(symbol, out var nameSpaceStart, out var nameSpaceEnd);

            var fullName = GeneratorTransformResult<ISymbol>.GetTypeFullName(symbol);
            var className = symbol.Name;
            var @namespace = GeneratorTransformResult<ISymbol>.GetNameSpace(symbol);
            if (!string.IsNullOrEmpty(@namespace))
            {
                @namespace = "global::" + @namespace;
            }
            var nullableEnd = symbol.IsReferenceType && (nullableEnable & NullableContext.Enabled) != 0 ? "?" : string.Empty;

            var ctxNullableEnd = (nullableEnable & NullableContext.Enabled) != 0 ? "?" : string.Empty;
            var command = string.Empty;
            var hasCommand = false;
            if (!logMode)
            {
                command = GenerateOnEventCommand(symbol, ctxNullableEnd, out var diagnostic);
                hasCommand = command != string.Empty;
                if (diagnostic != null)
                {
                    context.ReportDiagnostic(diagnostic);
                    outCode = null;
                    return false;
                }
            }
            var methodBody = new StringBuilder();
            var isEnable = symbol.GetAttribute(Consts.EventSourceGenerateAttribute.FullName)?
                .GetByNamed<bool>(Consts.EventSourceGenerateAttribute.UseIsEnable) ?? true;
            foreach (var item in methods)
            {
                var str = GenerateMethod(item, isEnable, logMode, out var diag);
                if (diag != null)
                {
                    context.ReportDiagnostic(diag);
                    outCode = null;
                    return false;
                }
                methodBody.AppendLine(str);
            }
            var classHasUnsafe = HasKeyword(symbol, SyntaxKind.UnsafeKeyword);
            var unsafeKeyword = classHasUnsafe ? "unsafe" : string.Empty;
            var interfaceBody = string.Empty;
            var imports = new List<string>();
            var attr = symbol.GetAttribute(Consts.EventSourceGenerateAttribute.FullName)!;
            var singletonExpression = string.Empty;

            if (!logMode&&attr.GetByNamed<bool>(Consts.EventSourceGenerateAttribute.GenerateSingleton))
            {
                //Check if not exists EventSourceAccesstorInstanceAttribute
                var accesstorInstances = symbol.GetMembers()
                    .Where(x => x is IFieldSymbol || x is IPropertySymbol)
                    .Where(x => x.HasAttribute(Consts.EventSourceAccesstorInstanceAttribute.FullName))
                    .ToList();
                if (accesstorInstances.Count > 1)
                {
                    for (int i = 0; i < accesstorInstances.Count; i++)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Messages.AccesstorInstanceMustOnlyOne, accesstorInstances[i].Locations[0]));
                    }
                    outCode = null;
                    return false;
                }
                var attrStr = string.Empty;
                if (accesstorInstances.Count == 0)
                {
                    attrStr = $"[{Consts.EventSourceAccesstorInstanceAttribute.FullName}]";
                }
                else
                {
                    var acc = accesstorInstances[0];
                    var isAccept = acc.IsStatic && (acc.DeclaredAccessibility.HasFlag(Accessibility.Internal) || acc.DeclaredAccessibility.HasFlag(Accessibility.Public));
                    if (!isAccept)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Messages.AccesstorInstanceDeclareError, acc.Locations[0]));

                        outCode = null;
                        return false;
                    }
                }
                singletonExpression = $"{attrStr} public static readonly global::{symbol} Instance = new global::{symbol}();";
            }
            if (!logMode && attr.GetByNamed<bool>(Consts.EventSourceGenerateAttribute.IncludeInterface))
            {
                var interfaceAccessibility = (Accessibility)attr.GetByNamed<int>(Consts.EventSourceGenerateAttribute.InterfaceVisilbility);
                if (interfaceAccessibility == Accessibility.NotApplicable)
                {
                    interfaceAccessibility = Accessibility.Public;
                }
                var interfaceAccessibilityStr = GeneratorTransformResult<ISymbol>.GetAccessibilityString(interfaceAccessibility);
                interfaceBody = $@"
{interfaceAccessibilityStr} interface I{symbol.Name}
{{
    {string.Join("\n", methods.Select(GenerateInterfaceMethod))}
}}
";
                imports.Add($"I{symbol.Name}");

            }
            var importExpression = string.Empty;
            if (imports.Count != 0)
            {
                importExpression = ":" + string.Join(",", imports);
            }
            var onEventCommandExecutedCode = string.Empty;
            if (hasCommand)
            {
                onEventCommandExecutedCode = "[global::System.Diagnostics.Tracing.NonEventAttribute] partial void OnEventCommandExecuted(global::System.Diagnostics.Tracing.EventCommandEventArgs command);";
            }
            var code = @$"
#pragma warning disable CS8604
#nullable enable
            {nameSpaceStart}

                {visibility} {unsafeKeyword} partial class {className} {importExpression}
                {{ 
                    {singletonExpression}

                    {methodBody}

                    {command}
                    {onEventCommandExecutedCode}
                }}
                {interfaceBody}
            }}
#nullable restore
#pragma warning restore
                ";

            outCode = Helpers.FormatCode(code);
            return true;
        }
        private static string GenerateInterfaceMethod(IMethodSymbol method)
        {
            return $@"{method.ReturnType} {method.Name}({string.Join(",", method.Parameters)});";
        }
        private static bool IsSupportCounterType(ITypeSymbol type)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_Int64:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                    return true;
                default:
                    return false;
            }
        }
        private static bool IsCounterType(ITypeSymbol type)
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
        private static bool IsEventCounterType(ITypeSymbol type)
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
        private static string GetCounterName(CounterTypes type)
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
        private static string GenerateCreateCounter(string left, string name, CounterTypes type, double displayRateTimeScaleMs, string? displayUnits, string? displayName, string? right)
        {
            var typeName = GetCounterName(type);
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = "global::System.String.Empty";
            }
            else
            {
                displayName = $"\"{displayName}\"";
            }
            if (string.IsNullOrEmpty(displayUnits))
            {
                displayUnits = "global::System.String.Empty";
            }
            else
            {
                displayUnits = $"\"{displayUnits}\"";
            }
            switch (type)
            {
                case CounterTypes.EventCounter:
                    return $"{left} new {typeName}(\"{name}\",this){{ DisplayName = {displayName},DisplayUnits={displayUnits} }}";
                case CounterTypes.IncrementingEventCounter:
                    return $"{left} new {typeName}(\"{name}\",this){{ DisplayName = {displayName},DisplayUnits={displayUnits}, DisplayRateTimeScale=global::System.TimeSpan.FromMilliseconds({displayRateTimeScaleMs})}}";
                case CounterTypes.PollingCounter:
                    return $"{left} new {typeName}(\"{name}\",this,{right}){{ DisplayName = {displayName},DisplayUnits={displayUnits} }}";
                case CounterTypes.IncrementingPollingCounter:
                    return $"{left} new {typeName}(\"{name}\",this,{right}){{ DisplayName = {displayName},DisplayUnits={displayUnits}, DisplayRateTimeScale=global::System.TimeSpan.FromMilliseconds({displayRateTimeScaleMs})}}";
                default:
                    throw new NotSupportedException(type.ToString());
            }
        }
        private static string GenerateOnEventCommand(INamedTypeSymbol symbol, string nullableTail, out Diagnostic? diagnostic)
        {
            var counter = symbol.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(x => x.HasAttribute(Consts.CounterAttribute.FullName))
                .ToList();
            if (counter.Count == 0)
            {
                diagnostic = null;
                return string.Empty;
            }
            var addins = new StringBuilder();
            var bodys = new StringBuilder();
            foreach (var item in counter)
            {
                var counterAttr = item.GetAttribute(Consts.CounterAttribute.FullName)!;
                var name = counterAttr.GetByIndex<string>(0)!;
                var type = counterAttr.GetByIndex<CounterTypes>(1);
                var displayRateTimeScaleMs = counterAttr.GetByNamed<double>(Consts.CounterAttribute.DisplayRateTimeScaleMs);
                var displayUnits = counterAttr.GetByNamed<string>(Consts.CounterAttribute.DisplayUnits);
                var displayName = counterAttr.GetByNamed<string>(Consts.CounterAttribute.DisplayName);
                //Debugger.Launch();
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
                if ((type == CounterTypes.IncrementingPollingCounter || type == CounterTypes.IncrementingEventCounter) && displayRateTimeScaleMs <= 0)
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
                    var specialName = GetSpecialName(item.Name);

                    var invokeBody = $"global::System.Threading.Interlocked.Read(ref {item.Name})";
                    var parType = item.Type;
                    if (parType.SpecialType != SpecialType.System_Int64)
                    {
                        invokeBody = $"global::System.Threading.Volatile.Read(ref {item.Name})";//Thread safe?
                    }
                    var addinBody = $@"    
if(inc==1)
    global::System.Threading.Interlocked.Increment(ref {item.Name});
else
    global::System.Threading.Interlocked.Add(ref {item.Name},inc);
";
                    if (parType.SpecialType == SpecialType.System_Single || parType.SpecialType == SpecialType.System_Double)
                    {
                        addinBody = $@"
    global::Diagnostics.Generator.Core.InterlockedHelper.Add(ref {item.Name},inc);
";
                    }
                    addins.AppendLine($@"
[global::System.Diagnostics.Tracing.NonEventAttribute]
public void Increment{specialName}({item.Type} inc=1)
{{
    {addinBody}
}}
");
                    bodys.AppendLine(GenerateCreateCounter($"{counterName} ??=", name, type, displayRateTimeScaleMs, displayUnits, displayName, $"()=> {invokeBody}") + ";");
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
             SpecialType.System_DateTime,
             SpecialType.System_Enum
        };
        private static bool IsSupportType(ITypeSymbol type)
        {
            if (supportTypes.Contains(type.SpecialType) || type.TypeKind == TypeKind.Enum)
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
        internal static bool HasKeyword(ISymbol symbol, SyntaxKind kind)
        {
            var syntax = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
            if (symbol == null)
            {
                return false;
            }
            if (syntax is MemberDeclarationSyntax m)
            {
                return m.Modifiers.Any(x => x.IsKind(kind));
            }
            if (syntax is ClassDeclarationSyntax c)
            {
                return c.Modifiers.Any(x => x.IsKind(kind));
            }
            if (syntax is StructDeclarationSyntax s)
            {
                return s.Modifiers.Any(x => x.IsKind(kind));
            }
            return false;
        }
        private static string GenerateMethod(IMethodSymbol method, bool useIsEnable, bool logMode, out Diagnostic? diagnostic)
        {
            var pars = method.Parameters;
            if (logMode)
            {
                pars = pars.Skip(1).ToImmutableArray();
            }
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
            var relatedActivityIds = pars.Where(x => x.HasAttribute(Consts.RelatedActivityIdAttribute.FullName)).ToList();
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
            if (!HasKeyword(method, SyntaxKind.UnsafeKeyword))
            {
                unsafeKeyWord = string.Empty;
            }
            if (actualDatasCount == 0)
            {
                datasDeclare = string.Empty;
                datasPar = "null";
            }
            var eventAttr = method.GetAttribute(Consts.EventAttribute.FullName);
            var eventId = eventAttr?.GetByIndex<int>(0);
            var attributes = string.Empty;
            if (logMode)
            {
                var loggerMessageAttr = method.GetAttribute(Consts.LoggerMessageAttribute.FullName);
                //Debugger.Launch();
                var logMessageData = LoggerMessageData.FromAttribute(method.Name, loggerMessageAttr, eventAttr);
                attributes = logMessageData.ToAttributeCode(eventAttr);
                eventId = logMessageData.EventId;
            }

            var invokeMethod = $"WriteEventWithRelatedActivityIdCore({eventId},{relate}, {actualDatasCount}, {datasPar});";
            if (actualDatasCount == 0)
            {
                invokeMethod = $"WriteEvent({eventId});";
            }
            var argList = string.Join(",", pars.Select(x => x.ToString()));
            var parExecuteDeclare = string.Empty;
            var invokeParDeclare = string.Empty;
            if (method.ReturnsVoid)
            {
                parExecuteDeclare = $"[global::System.Diagnostics.Tracing.NonEventAttribute] partial void On{method.Name}({argList});";
                invokeParDeclare = $"On{method.Name}({string.Join(",", pars.Select(x => x.Name))});";
            }

            var isEnableTop = string.Empty;
            var isEnableBegin = string.Empty;
            var isEnableEnd = string.Empty;
            if (useIsEnable)
            {
                isEnableTop = "if(IsEnabled())";
                isEnableBegin = "{";
                isEnableEnd = "}";
            }
            var partialKey = "partial";
            if (logMode)
            {
                partialKey = string.Empty;
            }
            var s = $@"
{attributes}
{GeneratorTransformResult<ISymbol>.GetAccessibilityString(method.DeclaredAccessibility)} {unsafeKeyWord} {partialKey} {method.ReturnType} {method.Name}({argList})
{{
    {isEnableTop}
    {isEnableBegin}
        {datasDeclare}
        {string.Join("\n", pars.Except(relatedActivityIds).Select(GenerateWrite))}
        {invokeMethod}
        {invokeParDeclare}
    {isEnableEnd}
}}
{parExecuteDeclare}
";
            diagnostic = null;
            return s;
        }
        private static string GenerateWrite(IParameterSymbol parameter, int index)
        {
            if (parameter.Type.SpecialType == SpecialType.System_String)
            {
                return $@"
datas[{index}] = new global::System.Diagnostics.Tracing.EventSource.EventData
{{
    DataPointer = {parameter.Name}==null?global::System.IntPtr.Zero:(nint)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::System.Runtime.InteropServices.MemoryMarshal.GetReference(global::System.MemoryExtensions.AsSpan({parameter.Name}))),
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
    }
    internal class LoggerMessageData
    {
#if false
        int? level = null;
                string message = string.Empty;
                string? eventName = null;
#endif

        public int EventId { get; set; }

        public int? Level { get; set; }

        public string Message { get; set; } = string.Empty;

        public string? EventName { get; set; }

        public string ToAttributeCode(AttributeData? eventAttr)
        {
            var eventPars = new StringBuilder();
            if (eventAttr != null && eventAttr.ConstructorArguments.Length != 0)
            {
                var hasMessage = false;
                var hasLevel = false;
                for (int i = 0; i < eventAttr.NamedArguments.Length; i++)
                {
                    eventPars.Append(",");
                    var item = eventAttr.NamedArguments[i];
                    switch (item.Key)
                    {
                        case "Message":
                            hasMessage = true;
                            eventPars.Append($"{item.Key} = \"{item.Value}\"");
                            break;
                        case "Level":
                            hasLevel = true;
                            eventPars.Append($"{item.Key} = (global::System.Diagnostics.Tracing.EventLevel){item.Value.ToCSharpString()}");
                            break;
                        case "Keywords":
                            eventPars.Append($"{item.Key} = (global::System.Diagnostics.Tracing.EventKeywords){item.Value.ToCSharpString()}");
                            break;
                        case "Opcode":
                            eventPars.Append($"{item.Key} = (global::System.Diagnostics.Tracing.EventOpcode){item.Value.ToCSharpString()}");
                            break;
                        case "Task":
                            eventPars.Append($"{item.Key} = (global::System.Diagnostics.Tracing.EventTask){item.Value.ToCSharpString()}");
                            break;
                        case "Channel":
                            eventPars.Append($"{item.Key} = (global::System.Diagnostics.Tracing.EventChannel){item.Value.ToCSharpString()}");
                            break;
                        case "Version":
                            eventPars.Append($"{item.Key} = (global::System.Byte){item.Value.ToCSharpString()}");
                            break;
                        case "Tags":
                            eventPars.Append($"{item.Key} = (global::System.Diagnostics.Tracing.EventTags){item.Value.ToCSharpString()}");
                            break;
                        case "ActivityOptions":
                            eventPars.Append($"{item.Key} = (global::System.Diagnostics.Tracing.EventActivityOptions){item.Value.ToCSharpString()}");
                            break;
                        default:
                            break;
                    }
                }
                if (Level != null && !hasLevel && TryLogLevelToEventLevel(Level, out var levelStr))
                {
                    eventPars.Append($",Level={levelStr}");
                }
                if (!hasMessage && !string.IsNullOrEmpty(Message))
                {
                    eventPars.Append($",Message=\"{Message}\"");
                }
            }
            else 
            {
                if (!string.IsNullOrEmpty(Message))
                    eventPars.Append($",Message=\"{Message}\"");
                if (Level != null && TryLogLevelToEventLevel(Level, out var levelStr))
                    eventPars.Append($",Level={levelStr}");
            }
            return $"[System.Diagnostics.Tracing.EventAttribute({EventId}{eventPars})]";
        }

        public static LoggerMessageData FromAttribute(string methodName,AttributeData? loggerMessageAttr,AttributeData? eventAttr)
        {
            int? eventId = eventAttr?.GetByIndex<int>(0);
            int? level = null;
            string message = string.Empty;
            string? eventName = null;
            if (loggerMessageAttr != null)
            {
                //https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Logging.Abstractions/gen/LoggerMessageGenerator.Parser.cs
                var items = loggerMessageAttr.ConstructorArguments;
                switch (items.Length)
                {
                    case 1:
                        // LoggerMessageAttribute(LogLevel level)
                        // LoggerMessageAttribute(string message)
                        if (items[0].Type!.SpecialType == SpecialType.System_String)
                        {
                            message = (string?)GetItem(items[0]) ?? string.Empty;
                            level = null;
                        }
                        else
                        {
                            message = string.Empty;
                            level = items[0].IsNull ? null : (int?)GetItem(items[0]);
                        }
                        break;

                    case 2:
                        // LoggerMessageAttribute(LogLevel level, string message)
                        level = items[0].IsNull ? null : (int?)GetItem(items[0]);
                        message = items[1].IsNull ? string.Empty : (string?)GetItem(items[1]) ?? string.Empty;
                        break;

                    case 3:
                        // LoggerMessageAttribute(int eventId, LogLevel level, string message)
                        if (!items[0].IsNull)
                        {
                            eventId = (int?)GetItem(items[0]);
                        }
                        level = items[1].IsNull ? null : (int?)GetItem(items[1]);
                        message = items[2].IsNull ? string.Empty : (string?)GetItem(items[2]) ?? string.Empty;
                        break;

                    default:
                        //Debug.Assert(false, "Unexpected number of arguments in attribute constructor.");
                        break;
                }

                if (loggerMessageAttr.NamedArguments.Any())
                {
                    foreach (KeyValuePair<string, TypedConstant> namedArgument in loggerMessageAttr.NamedArguments)
                    {
                        TypedConstant typedConstant = namedArgument.Value;
                        if (typedConstant.Kind == TypedConstantKind.Error)
                        {
                            break; // if a compilation error was found, no need to keep evaluating other args
                        }
                        else
                        {
                            TypedConstant value = namedArgument.Value;
                            switch (namedArgument.Key)
                            {
                                case "EventId":
                                    eventId = (int?)GetItem(value);
                                    break;
                                case "Level":
                                    level = value.IsNull ? null : (int?)GetItem(value);
                                    break;
                                case "EventName":
                                    eventName = (string?)GetItem(value);
                                    break;
                                case "Message":
                                    message = value.IsNull ? string.Empty : (string?)GetItem(value) ?? string.Empty;
                                    break;
                            }
                        }
                    }
                }
                //EventId map to or eventAttr.EventId or 
                if (eventAttr != null)
                {
                    eventId = eventAttr.GetByIndex<int>(0);
                }
                else if (eventId == null)
                {
                    //https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Logging.Abstractions/gen/LoggerMessageGenerator.Parser.cs#L236C51-L236C149
                    eventId = GetNonRandomizedHashCode(string.IsNullOrWhiteSpace(eventName) ? methodName : eventName!);
                }
            }
            return new LoggerMessageData { EventId = eventId.Value, EventName = eventName, Level = level, Message = message };
        }
        private static bool TryLogLevelToEventLevel(int? level, out string? eventLevelStr)
        {
            if (level == null)
            {
                eventLevelStr = null;
                return false;
            }
            var head = "global::System.Diagnostics.Tracing.EventLevel.";
            string? tail;
            switch (level)
            {
                case 0:
                case 1:
                    tail = "Verbose";
                    break;
                case 2:
                    tail = "Informational";
                    break;
                case 3:
                    tail = "Warning";
                    break;
                case 4:
                    tail = "Error";
                    break;
                case 5:
                    tail = "Critical";
                    break;
                default:
                    eventLevelStr = null;
                    return false;
            }
            eventLevelStr = head + tail;
            return true;
        }

        private static object? GetItem(TypedConstant arg) => arg.Kind == TypedConstantKind.Array ? arg.Values : arg.Value;
        //https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Logging.Abstractions/gen/LoggerMessageGenerator.Parser.cs#L854C8-L862C10
        internal static int GetNonRandomizedHashCode(string s)
        {
            uint result = 2166136261u;
            foreach (char c in s)
            {
                result = (c ^ result) * 16777619;
            }
            return Math.Abs((int)result);
        }
    }
}
