using Microsoft.CodeAnalysis;
using System.Collections.Generic;
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
                .Where(x => x.HasAttribute(Consts.CounterItemAttribute.Name))
                .ToList();
            if (fields.Count == 0)
            {
                return;
            }
            var executedNames = new HashSet<string>();
            foreach (var item in fields)
            {
                if (!executedNames.Add(item.GetAttribute(Consts.CounterItemAttribute.Name)!.GetByIndex<string>(0)!))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Messages.TheIdHasSames, item.Locations[0]));
                }
            }
            var specialNames = CreateSpecialNameMap(fields);
            var attr = symbol.GetAttribute(Consts.CounterMappingAttribute.Name)!;
            var forAnyProviders = attr.GetByNamed<bool>(Consts.CounterMappingAttribute.ForAnysProviders);
            var forProviders = attr.GetByNamed<string[]>(Consts.CounterMappingAttribute.ForProviders);

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
        public async global::System.Threading.Tasks.Task<{symbol.Name}> OnceAsync(global::System.Threading.CancellationToken token = default)
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
                return this;
            }}
            finally
            {{
                Changed -= handler;
            }}
        }}
        public async global::System.Threading.Tasks.Task OnceAsync(global::System.Action<{symbol.Name}> action, global::System.Threading.CancellationToken token = default)
        {{
            await OnceAsync(token);
            action?.Invoke(this);
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
            {string.Join("\n", fields.Select(x => WriteWriteToItem(x, "tw", specialNames)))}
            
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
{nameSpaceEnd}
#pragma warning restore IDE0039
{nullableDeclareEnd}
";

            code = Helpers.FormatCode(code);
            context.AddSource($"{className}.g.cs", code);
        }
        private string WriteWriteToItem(IFieldSymbol field, string textWriterName, IDictionary<string, string> specialNames)
        {
            var propName = specialNames[field.Name];
            return $@"var t{field.Name}={propName};
            if(t{field.Name} != null)
                {textWriterName}.WriteLine(""{{0}} {{1}} {{2}}"", t{field.Name}.DisplayName, t{field.Name}.Value,t{field.Name}.Unit);";
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
            var attr = symbol.GetAttribute(Consts.CounterItemAttribute.Name);
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
