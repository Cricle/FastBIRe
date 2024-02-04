using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;

namespace Diagnostics.Generator.Internal
{
    internal class ActivityMapParse: ParserBase
    {
        public void Execute(SourceProductionContext context, GeneratorTransformResult<ISymbol> node)
        {
            var attr = node.SyntaxContext.TargetSymbol.GetAttribute(Consts.MapToActivityAttribute.FullName);
            var symbol = (INamedTypeSymbol)attr!.GetByIndex<ISymbol>(0)!;
            var withCallTog = attr!.GetByNamed<bool>(Consts.MapToActivityAttribute.WithEventSourceCall);
            var callEventAtEnd = attr!.GetByNamed<bool>(Consts.MapToActivityAttribute.CallEventAtEnd);

            var source = (INamedTypeSymbol)node.Value;

            var hasAccesstor = false;
            var instanceAccesstCode = string.Empty;
            if (withCallTog)
            {
                var sourceEventSourceGenerateAttr = source.GetAttribute(Consts.EventSourceGenerateAttribute.FullName);
                var isGenerateInstance = false;
                if (sourceEventSourceGenerateAttr != null)
                {
                    isGenerateInstance = sourceEventSourceGenerateAttr.GetByNamed<bool>(Consts.EventSourceGenerateAttribute.GenerateSingleton);
                }
                if (isGenerateInstance)
                {
                    instanceAccesstCode = $"global::{source}.Instance";
                    hasAccesstor = true;
                }
                else
                {
                    var accessInstance = source.GetMembers()
                        .Where(x => x is IFieldSymbol || x is IPropertySymbol)
                        .Where(x => x.HasAttribute(Consts.EventSourceAccesstorInstanceAttribute.FullName))
                        .FirstOrDefault();
                    if (accessInstance != null)
                    {
                        instanceAccesstCode = $"global::{source}.{accessInstance.Name}";
                        hasAccesstor = true;
                    }
                }
            }

            if (!hasAccesstor&& withCallTog)
            {
                context.ReportDiagnostic(Diagnostic.Create(Messages.NoAccesstorNoCallTogether, source.Locations[0], source.Name));
                return;
            }

            var eventSourceEvents = source.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(x => x.HasAttribute(Consts.EventAttribute.FullName) && !x.HasAttribute(Consts.ActivityIgnoreAttribute.FullName))
                .ToList();

            var nullableEnable = symbol.GetNullableContext(node.SemanticModel);
            var visibility = GetVisiblity(symbol);
            node.GetWriteNameSpace(out var nameSpaceStart, out var nameSpaceEnd);

            var fullName = node.GetTypeFullName();
            var @namespace = node.GetNameSpace();
            if (!string.IsNullOrEmpty(@namespace))
            {
                @namespace = "global::" + @namespace;
            }
            var nullableEnd = symbol.IsReferenceType && (nullableEnable & NullableContext.Enabled) != 0 ? "?" : string.Empty;

            var ctxNullableEnd = (nullableEnable & NullableContext.Enabled) != 0 ? "?" : string.Empty;

            var code = @$"
#pragma warning disable CS8604
#nullable enable
            {nameSpaceStart}
                [global::Diagnostics.Generator.Core.Annotations.ActivityMapToEventSourceAttribute(typeof(global::{source}),{eventSourceEvents.Count})]
                {visibility} partial class {symbol.Name}
                {{
                    {string.Join("\n", eventSourceEvents.Select(x=> WriteMethodMap(symbol.IsStatic,x, ctxNullableEnd, withCallTog, instanceAccesstCode, callEventAtEnd)))}
                }}
            }}
#nullable restore
#pragma warning restore
                ";

            code = Helpers.FormatCode(code);
            context.AddSource($"{symbol.Name}.ActivityMap.g.cs", code);
        }
        private string WriteMethodMap(bool isStatic,IMethodSymbol method,string nullableEnd,bool withCallTog,string eventSource,bool callEventAtEnd)
        {
            var visibility = GeneratorTransformResult<ISymbol>.GetAccessibilityString(method.DeclaredAccessibility);
            var staticKeyword = isStatic ? "static":string.Empty;
            var tags = method.GetAttributes(Consts.ActivityTagAttribute.FullName);
            var isNoEvent = method.HasAttribute(Consts.ActivityNoEventAttribute.FullName);
            if (tags.Count == 0 && isNoEvent)
            {
                return string.Empty;
            }
            string? eventTagCodes;
            if (method.Parameters.Length!=0)
            {
                var methodArgs = string.Join("\n", method.Parameters.Select(x => $"tags[\"{x.Name}\"] = {x.Name};"));
                eventTagCodes = $@"
tags = new global::System.Diagnostics.ActivityTagsCollection();
{methodArgs}
if(additionTags != null)
{{
    foreach(global::System.Collections.Generic.KeyValuePair<global::System.String, global::System.Object{nullableEnd}> tag in additionTags)
    {{
         if (tag.Key != null)
        {{
            tags[tag.Key] = tag.Value;
        }}
    }}
}}
";
            }
            else
            {
                eventTagCodes = @"
if(additionTags != null)
{
    tags = new global::System.Diagnostics.ActivityTagsCollection(additionTags);
}
";
            }
            var inputs = method.Parameters.Select(x => x.ToString());
            var inputJoined = string.Join(",", inputs);
            var additionArgs = string.Empty;
            var invokeArgsJoined = string.Empty;
            if (method.Parameters.Length != 0)
            {
                additionArgs = $"{inputJoined},";
                invokeArgsJoined = string.Join(",", method.Parameters.Select(x => x.Name)) + ",";
            }
            var tagCodes = string.Empty;
            if (tags.Count != 0)
            {
                var s = new StringBuilder();
                foreach ( var tag in tags) 
                {
                    var tagMethod = "SetTag";
                    var name = tag.GetByIndex<string>(0);
                    var expression = tag.GetByIndex<string>(1);
                    var isSet = tag.GetByNamed<bool?>(Consts.ActivityTagAttribute.IsSet);
                    if (!isSet.GetValueOrDefault())
                    {
                        tagMethod = "AddTag";
                    }
                    if (method.Parameters.Length != 0)
                    {
                        s.AppendLine($"activity.{tagMethod}(\"{name}\",global::System.String.Format(\"{expression}\",{string.Join(",", method.Parameters.Select(x => x.Name))}));");
                    }
                    else
                    {
                        s.AppendLine($"activity.{tagMethod}(\"{name}\",\"{expression}\");");
                    }
                }
                tagCodes = s.ToString();
            }

            var endsArgs = $"{additionArgs} global::System.DateTimeOffset timestamp = default, global::System.Collections.Generic.IEnumerable<global::System.Collections.Generic.KeyValuePair<global::System.String, global::System.Object{nullableEnd}>>{nullableEnd} additionTags = null";

            var eventSourceParInvokeJoined = string.Join(",", method.Parameters.Select(x => x.Name));

            var invokeCodes = "timestamp,additionTags";
            if (method.Parameters.Length!=0)
            {
                invokeCodes = eventSourceParInvokeJoined + "," + invokeCodes;
            }
            var eventId = method.GetAttribute(Consts.EventAttribute.FullName)!
                .GetByIndex<int>(0);
            var activityMapToEventAttr = $"[global::Diagnostics.Generator.Core.Annotations.ActivityMapToEventAttribute({eventId},\"{method.Name}\",new global::System.Type[]{{ {string.Join(",",method.Parameters.Select(x=>$"typeof({x.Type})"))} }})]";
            var argCode = $"global::System.Diagnostics.Activity{nullableEnd} activity, {endsArgs}";

            var callEventSourceCodes = string.Empty;
            if (withCallTog)
            {
                callEventSourceCodes = $"{eventSource}.{method.Name}({eventSourceParInvokeJoined});";
            }
            var beginCallEventSourceCodes = string.Empty;
            var endCallEventSourceCodes = string.Empty;
            if (callEventAtEnd)
            {
                endCallEventSourceCodes = callEventSourceCodes;
            }
            else
            {
                beginCallEventSourceCodes = callEventSourceCodes;
            }
            return $@"
{activityMapToEventAttr}
{visibility} {staticKeyword} {method.ReturnType} {method.Name}({endsArgs})
{{
    {method.Name}(global::System.Diagnostics.Activity.Current,{invokeCodes});
}}
{activityMapToEventAttr}
{visibility} {staticKeyword} {method.ReturnType} {method.Name}({argCode})
{{
    {beginCallEventSourceCodes}
    if(activity != null)
    {{
        global::System.Diagnostics.ActivityTagsCollection{nullableEnd} tags = null;
        {eventTagCodes}
        activity.AddEvent(new global::System.Diagnostics.ActivityEvent(""{method.Name}"", timestamp, tags));
        {tagCodes}
        On{method.Name}(activity,{invokeArgsJoined}timestamp, additionTags);
    }}
    {endCallEventSourceCodes}
}}
{staticKeyword} partial {method.ReturnType} On{method.Name}({argCode});
";
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
