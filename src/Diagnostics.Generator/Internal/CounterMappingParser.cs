using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Diagnostics.Generator.Internal
{
    internal class CounterMappingParser : ParserBase
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

            var fields = symbol.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(x => x.HasAttribute(Consts.CounterItemAttribute.FullName))
                .ToList();
            if (fields.Count == 0)
            {
                return;
            }
            var executedNames = new HashSet<string>();
            foreach (var item in fields)
            {
                var id = item.GetAttribute(Consts.CounterItemAttribute.FullName)!.GetByIndex<string>(0)!;
                if (!executedNames.Add(id))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Messages.TheIdHasSames, item.Locations[0], id));
                }
            }
            var specialNames = CreateSpecialNameMap(fields);
            var attr = symbol.GetAttribute(Consts.CounterMappingAttribute.FullName)!;
            var forAnyProviders = attr.GetByNamed<bool>(Consts.CounterMappingAttribute.ForAnysProviders);
            var forProviders = attr.GetByNamedArray<string>(Consts.CounterMappingAttribute.ForProviders);
            var withInterval = attr.GetByNamed<bool>(Consts.CounterMappingAttribute.WithInterval);
            var withCreator = attr.GetByNamed<bool>(Consts.CounterMappingAttribute.WithCreator);
            var creatorHasInstance= attr.GetByNamed<bool>(Consts.CounterMappingAttribute.CreatorHasInstance);

            if (forAnyProviders && (forProviders == null || forProviders.Length == 0))
            {
                context.ReportDiagnostic(Diagnostic.Create(Messages.ForProviderMustInput, symbol.Locations[0]));
                return;
            }

            var forProviderInits = string.Empty;
            var forProviderChecks = "payload.Name != null && valueSetter.TryGetValue(payload.Name, out var setter)";
            //Debugger.Launch();
            var supportProviderCount = forAnyProviders ? "null" : (forProviders?.Length ?? 0).ToString();
            if (forProviders != null && forProviders.Length != 0)
            {
                forProviderChecks = $"(payload.TraceEvent!=null&&forProviderNames.Contains(payload.TraceEvent.ProviderName))&&" + forProviderChecks;
                forProviderInits = string.Join(",", forProviders.Select(x => $"\"{x}\""));
            }
            var nullableDeclareStart = string.Empty;
            var nullableDeclareEnd = string.Empty;
            if (!string.IsNullOrEmpty(ctxNullableEnd))
            {
                nullableDeclareStart = "#nullable enable";
                nullableDeclareEnd = "#nullable restore";
            }
            var mapImpl = $@"
#if NET8_0_OR_GREATER
global::System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary(
#endif
new global::System.Collections.Generic.Dictionary<global::System.String, global::System.Action<{symbol.Name}, global::Diagnostics.Helpers.ICounterPayload>>
{{
    {string.Join("\n", fields.Select(x=>WriteWriteMethod(x,nullableEnd)))}
}}
#if NET8_0_OR_GREATER
)
#endif
";

            var incrCodes = string.Empty;
            if (withInterval)
            {
                incrCodes = $@"
public partial class Interval{symbol.Name} : {symbol.Name}, global::System.IDisposable
{{
    private global::System.Int32 isChanged;
    private readonly global::System.Threading.Timer timer;

    public Interval{symbol.Name}(global::System.TimeSpan interval)
    {{
        timer = new global::System.Threading.Timer(OnTimerRaise, this, global::System.TimeSpan.Zero, interval);
    }}
    protected override void OnUpdated(global::Diagnostics.Helpers.ICounterPayload payload)
    {{
        global::System.Threading.Interlocked.CompareExchange(ref isChanged, 1, 0);
    }}
    private void OnTimerRaise(global::System.Object? state)
    {{
        if (global::System.Threading.Interlocked.CompareExchange(ref isChanged, 0, 1) == 1)
        {{
            RaiseChanged();
            RaisedTimerChanged(state);
        }}
        else
        {{
            RaisedTimerNoChanged(state);
        }}
    }}
    partial void RaisedTimerNoChanged(global::System.Object? state);
    partial void RaisedTimerChanged(global::System.Object? state);
    public void Dispose()
    {{
        timer.Dispose();
        OnDisposed();
    }}
    partial void OnDisposed();
}}
";
            }
            var creatorCode = string.Empty;
            //Debugger.Launch();
            if (withCreator)
            {
                var providers = symbol.GetAttributes(Consts.EventPipeProviderAttribute.FullName);
                if (providers.Count != 0)
                {
                    var providerNames = providers.Select(x => x.GetByIndex<string>(0));
                    var providerNameJoined = string.Join(",", providerNames.Select(x=>$"\"{x}\""));
                    var intervalCreate = $"throw new global::System.NotSupportedException(\"The {symbol.Name} has not support interval sample\");";
                    if (withInterval)
                    {
                        intervalCreate = $"return new Interval{symbol.Name}(interval);";
                    }
                    var instanceCode = string.Empty;
                    if (creatorHasInstance)
                    {
                        instanceCode = $"public static readonly {symbol.Name}EventSampleCreator Instance = new {symbol.Name}EventSampleCreator();";
                    }
                    var builderCodes = new List<string>();
                    var builderIndex = 0;
                    foreach (var item in providers)
                    {
                        var builderName = "builder" + builderIndex;
                        builderIndex++;
                        var name = item.GetByIndex<string>(0);
                        var level = item.GetByIndex<int>(1);
                        var keywords = item.GetByNamed<long?>(Consts.EventPipeProviderAttribute.Keywords);
                        var arguments = item.GetByNamed<string>(Consts.EventPipeProviderAttribute.Arguments);

                        var levelString = IntToLevel(level);
                        if (levelString == null)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Messages.LevelNotSpport, item.ApplicationSyntaxReference?.GetSyntax().GetLocation(), level));
                            return;
                        }
                        var keywordInvoke = string.Empty;
                        if (keywords!=null)
                        {
                            keywordInvoke = $"{builderName}.Keywords = {keywords};";
                        }

                        var argumentCodes = string.Empty;
                        if (string.IsNullOrEmpty(arguments))
                        {
                            argumentCodes = $"{builderName}.Arguments = new global::System.Collections.Generic.Dictionary<global::System.String, global::System.String>(1){{ [\"EventCounterIntervalSec\"] = \"1\" }}; ";
                        }
                        else
                        {
                            //Debugger.Launch();
                            if (!TryParse(arguments!, out var res))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(Messages.FailToParseArguments, item.ApplicationSyntaxReference?.GetSyntax().GetLocation(), arguments));
                                return;
                            }
                            argumentCodes = $@"{builderName}.Arguments = new global::System.Collections.Generic.Dictionary<global::System.String, global::System.String>
{{
    {res}
}};";
                        }
                        builderCodes.Add($@"
var {builderName} = new global::Diagnostics.Helpers.EventPipeProviderBuilder(""{name}"");
{builderName}.EventLevel = {levelString};
{keywordInvoke}
{argumentCodes}

builderConfig?.Invoke({builderName});
yield return {builderName}.Build();
");
                    }

                    var builderCodesJoined = string.Join("\n", builderCodes);
                    var builderCodeJoined = string.Join("\n", builderCodes);
                    creatorCode = $@"
    public partial class {symbol.Name}EventSampleCreator : global::Diagnostics.Helpers.IEventSampleCreator
    {{
        private static readonly global::System.Collections.Generic.HashSet<global::System.String> providerNames = new global::System.Collections.Generic.HashSet<global::System.String> {{ {providerNameJoined} }};
            
        {instanceCode}
        
        public global::System.Collections.Generic.IEnumerable<string> ProviderNames => providerNames;

        public global::System.Boolean SupportIntervalCounterProvider => {withInterval.ToBoolKeyword()};

        public global::Diagnostics.Helpers.IEventCounterProvider CreateCounterProvider()
        {{
            return new {symbol.Name}();
        }}

        public global::Diagnostics.Helpers.IEventCounterProvider CreateIntervalCounterProvider(global::System.TimeSpan interval)
        {{
            {intervalCreate}
        }}

        public global::Diagnostics.Helpers.ISampleProvider GetIntervalSample(global::Diagnostics.Helpers.ICounterResult counterResult, global::System.TimeSpan interval)
        {{
            return new global::Diagnostics.Helpers.SampleResult<{symbol.Name}>(counterResult, (Interval{symbol.Name})CreateCounterProvider());
        }}

        public global::System.Collections.Generic.IEnumerable<global::Microsoft.Diagnostics.NETCore.Client.EventPipeProvider> GetProviders(global::System.Action<global::Diagnostics.Helpers.IEventPipeProviderBuilder>? builderConfig = null)
        {{
            {builderCodesJoined}
        }}

        public global::Diagnostics.Helpers.ISampleProvider GetSample(global::Diagnostics.Helpers.ICounterResult counterResult)
        {{
            return new global::Diagnostics.Helpers.SampleResult<{symbol.Name}>(counterResult, ({symbol.Name})CreateCounterProvider());
        }}

        public global::System.Boolean IsAcceptProvider(global::System.String name)
        {{
            return providerNames.Contains(name);
        }}
    }}
";
                }
                else
                {
                    context.ReportDiagnostic(Diagnostic.Create(Messages.NoProvider, symbol.Locations[0]));
                }
            }
            var code = $@"
#pragma warning disable IDE0039
{nullableDeclareStart}
{nameSpaceStart}
    public partial class {symbol.Name}
    {{
        private static readonly global::System.Collections.Generic.IReadOnlyDictionary<global::System.String, global::System.Action<{symbol.Name}, global::Diagnostics.Helpers.ICounterPayload>> valueSetter = {mapImpl};
            
        public static global::System.Collections.Generic.IEnumerable<global::System.String> SupportEventNames => valueSetter.Keys;

        public static global::System.Boolean IsSupportEventName(global::System.String name)
        {{
            return valueSetter.ContainsKey(name);
        }}
        
        {string.Join("\n", fields.Select(x => WriteProperty(x, ctxNullableEnd, specialNames)))}
        
        public static global::System.Int32? SupportProviderCount => {supportProviderCount};

        public static global::System.Boolean ForAnysProviders => {forAnyProviders.ToBoolKeyword()};

        private static readonly global::System.Collections.Generic.HashSet<string> forProviderNames = new global::System.Collections.Generic.HashSet<string>
        {{
            {forProviderInits}
        }};

        public static global::System.Collections.Generic.IEnumerable<global::System.String> ForProviderNames => forProviderNames;
        
        public static global::System.Boolean IsSupportProviderNames(global::System.String name)
        {{
            return forProviderNames.Contains(name);
        }}

        public global::System.Boolean AllNotNull => {string.Join("&&\n", fields.Select(x => $"{specialNames[x.Name]} != null"))};

        public void Reset()
        {{
            {string.Join("\n", fields.Select(x=>WriteWriteNull(x,nullableEnd)))}
        }}
        public event global::System.EventHandler{ctxNullableEnd} Changed;
        public async global::System.Threading.Tasks.Task OnceAsync(global::System.Threading.CancellationToken token = default)
        {{
#if NETSTANDARD2_0
            var taskSource = new global::System.Threading.Tasks.TaskCompletionSource<bool>();
#else
            var taskSource = new global::System.Threading.Tasks.TaskCompletionSource();
#endif
            if (token.CanBeCanceled)
            {{
                token.Register(() => taskSource.TrySetCanceled());
            }}
            global::System.EventHandler handler = (_, __) =>
            {{
                if (AllNotNull)
                {{
#if NETSTANDARD2_0
                    taskSource.TrySetResult(true);
#else
                    taskSource.TrySetResult();
#endif
                    
                }}
            }};
            Changed += handler;
            try
            {{
                await taskSource.Task;
            }}
            finally
            {{
                Changed -= handler;
            }}
        }}
        protected void RaiseChanged()
        {{
            Changed?.Invoke(this, global::System.EventArgs.Empty);
            OnRaiseChanged();
        }}

        partial void OnRaiseChanged();

        public {symbol.Name} Copy()
        {{
            return ({symbol.Name})MemberwiseClone();
        }}

        public void Update(global::Diagnostics.Helpers.ICounterPayload payload)
        {{
            if ({forProviderChecks})
            {{
                setter(this, payload);
                OnUpdated(payload);
                OnPlayloadUpdated(payload);
            }}
        }}
    
        partial void OnPlayloadUpdated(global::Diagnostics.Helpers.ICounterPayload payload);

        protected virtual void OnUpdated(global::Diagnostics.Helpers.ICounterPayload payload)
        {{
            RaiseChanged();
        }}
        public virtual void WriteTo(global::System.IO.TextWriter tw)
        {{
            {WriteCalcDisplayLength(fields,"t","maxDisplayLength",specialNames)}
            {string.Join("\n", fields.Select(x => WriteWriteToItem(x, "tw","t", "maxDisplayLength")))}
            
            OnWriteTo(tw);
        }}
        partial void OnWriteTo(global::System.IO.TextWriter tw);
        public override global::System.String ToString()
        {{
            var sb = new global::System.IO.StringWriter();
            WriteTo(sb);
            return sb.ToString();
        }}
    }}
    {incrCodes}
    {creatorCode}
{nameSpaceEnd}
#pragma warning restore IDE0039
{nullableDeclareEnd}
";

            code = Helpers.FormatCode(code);
            context.AddSource($"{className}.g.cs", code);
        }
        private static bool TryParse(string input, out string? result)
        {
            var isInQuto = false;
            string? left = null;
            var eqIndex = 0;
            var eqs = new List<string>();
            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];
                if (c == '\"')
                {
                    isInQuto = !isInQuto;
                }
                else if (c == '=' && !isInQuto)
                {
                    left = input.Substring(eqIndex, i - eqIndex);
                    eqIndex = i;
                }
                else if ((c == ',' || input.Length == i + 1) && !isInQuto)
                {
                    if (left == null)
                    {
                        result = null;
                        return false;
                    }
                    var rightLeft = eqIndex + 1;
                    var rightRight = i - (c == ',' ? 1 : 0);
                    if (rightRight < rightLeft)
                    {
                        result = null;
                        return false;
                    }
                    var right = input.Substring(rightLeft, rightRight - rightLeft);
                    if (left.StartsWith("\"") && left.EndsWith("\""))
                    {
                        left = left.Substring(1, left.Length - 2);
                    }
                    if (right.StartsWith("\"") && right.EndsWith("\""))
                    {
                        right = right.Substring(1, right.Length - 2);
                    }
                    eqs.Add($"[\"{left}\"] = \"{right}\"");
                    eqIndex = i + 1;
                }
            }

            if (isInQuto)
            {
                result = null;
                return false;
            }
            result = string.Join(",\n", eqs);
            return true;
        }

        private string? IntToLevel(int level)
        {
            string? tail = null;
            switch (level)
            {
                case 0: tail = "LogAlways"; break;
                case 1: tail = "Critical"; break;
                case 2: tail = "Error"; break;
                case 3: tail = "Warning"; break;
                case 4: tail = "Informational"; break;
                case 5: tail = "Verbose"; break;
                default:
                    break;
            }
            if (tail==null)
            {
                return null;
            }
            return $"global::System.Diagnostics.Tracing.EventLevel.{tail}";
        }
        private string WriteCalcDisplayLength(IEnumerable<IFieldSymbol> fields,string copyVarHead,string varName, IDictionary<string, string> specialNames)
        {
            return $@"
global::System.Int32 {varName} = 0;
{string.Join("\n",fields.Select(x=>$"var {copyVarHead}{x.Name}={specialNames[x.Name]};"))}
{string.Join("\n", fields.Select(x => $"{varName} = global::System.Math.Max({varName},{copyVarHead}{x.Name}?.DisplayName?.Length??0);"))}
";
        }
        private string WriteWriteToItem(IFieldSymbol field, string textWriterName, string copyVarHead,string diaplayMaxVar)
        {
            var format = $"{{{{0,-{{{diaplayMaxVar}}}}}}} {{{{1}}}} {{{{2}}}}";
            return $@"if({copyVarHead}{field.Name} != null)
                {textWriterName}.WriteLine($""{format}"", {copyVarHead}{field.Name}.DisplayName, t{field.Name}.Value,{copyVarHead}{field.Name}.Unit);";
        }
        private IDictionary<string, string> CreateSpecialNameMap(IList<IFieldSymbol> fields)
        {
            var map = new Dictionary<string, string>(fields.Count);
            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                map[field.Name] = GetSpecialName(field.Name);
            }
            return map;
        }
        private string WriteWriteNull(IFieldSymbol symbol,string nullableEnd)
        {
            return $"global::System.Threading.Volatile.Write<global::Diagnostics.Helpers.ICounterPayload{nullableEnd}>(ref {symbol.Name},null);";
        }
        private string WriteProperty(IFieldSymbol symbol, string nullableEnd, IDictionary<string, string> specialNames)
        {
            return $"public global::Diagnostics.Helpers.ICounterPayload{nullableEnd} {specialNames[symbol.Name]} => global::System.Threading.Volatile.Read<global::Diagnostics.Helpers.ICounterPayload{nullableEnd}>(ref {symbol.Name});";
        }
        private string WriteWriteMethod(IFieldSymbol symbol,string nullableTail)
        {
            var attr = symbol.GetAttribute(Consts.CounterItemAttribute.FullName);
            var eventName = attr!.GetByIndex<string>(0);
            return $"[\"{eventName}\"] = (t,p)=>global::System.Threading.Volatile.Write<global::Diagnostics.Helpers.ICounterPayload{nullableTail}>(ref t.{symbol.Name},p),";
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
