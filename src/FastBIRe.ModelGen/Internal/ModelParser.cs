using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace FastBIRe.ModelGen.Internal
{
#if false
    [RecordToAttribute(,)]
    internal class xxxModel:IRecordToObject<>
    {
        public static readonly xxxModel Instance = new xxxModel();

        private xxxModel()
        {
            RecordToObjectManager<T>.SetRecordToObject(this);
        }
        
        public void Config(ITableBuilder builder)
        {
            builder.DateTimeColumn("datetime", nullable: false);
            builder.Column("namehash", DbType.Int64, nullable: false);
            builder.Column("count", DbType.Int32, nullable: false);
        }
        //ORM

        public xxx? To(IDataRecord record)
        {
            var obj = new xxx();

            return obj;
        }

        public IList<xxx?> ToList(IDataReader reader)
        {
            var res = new List<xxx?>();
            while(reader.Read())
            {
                res.Add(To(reader));
            }
            return res;
        }

    }
#endif
    internal class ModelParser
    {
        public void Execute(SourceProductionContext context, GeneratorTransformResult<ISymbol> node)
        {
            var symbol = (INamedTypeSymbol)node.SyntaxContext.TargetSymbol;
            var nullableEnable = symbol.GetNullableContext(node.SemanticModel);
            var visibility = GetVisiblity(symbol);
            node.GetWriteNameSpace(out var nameSpaceStart, out var nameSpaceEnd);

            var fullName = node.GetTypeFullName();
            var className = $"{symbol.Name}Model";
            var nullableEnd = (symbol.IsReferenceType&&(nullableEnable & NullableContext.Enabled) != 0) ? "?" : string.Empty;

            var props = symbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(x => x.DeclaredAccessibility== Accessibility.Public&&!x.HasAttribute(Consts.CompilerGeneratedAttribute.FullName)&&!x.HasAttribute(Consts.IgnoreAttribute.FullName))
                .Select(x => new PropertyModelInfo(x))
                .ToImmutableArray();
            var hasFail = false;
            foreach (var prop in props) 
            {
                if (prop.Symbol.IsReadOnly||prop.Symbol.IsWriteOnly)
                {
                    hasFail = true;
                    context.ReportDiagnostic(Diagnostic.Create(Messages.PropertyMustReadAndWrite, prop.Symbol.Locations[0], prop.Symbol.Name));
                }
                if (!prop.IsSupportType)
                {
                    hasFail = true;
                    context.ReportDiagnostic(Diagnostic.Create(Messages.DataTypeMustBeKnowTypesOrCustomer, prop.Symbol.Locations[0], prop.Symbol.Name));
                    Debugger.Launch();
                }
            }
            if (hasFail)
            {
                return;
            }
            var afterCallMethods = symbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(x => !x.IsGenericMethod&&x.HasAttribute(Consts.CreateAfterMethodAttribute.FullName))
                .Select(x => new MethodModelInfo(x))
                .ToImmutableArray();

            foreach (var item in afterCallMethods)
            {
                if (item.Symbol.DeclaredAccessibility != Accessibility.Public )
                {
                    hasFail = true;
                    context.ReportDiagnostic(Diagnostic.Create(Messages.AfterCallMethodMustNoArgumentAndMustPublic, item.Symbol.Locations[0]));
                }
            }
            if (hasFail)
            {
                return;
            }

            var keySet = string.Empty;
            if (props.Any(x=>x.IsKey))
            {
                var keyColumns = props.Where(x => x.IsKey)
                    .Select(x => $"\"{x.Name}\"");
                //TODO: key named
                keySet = $"builder.SetPrimaryKey(new System.String[] {{ {string.Join(",",keyColumns)} }});";
            }

            var indexSet = string.Empty;
            if (props.Any(x => x.IsIndex))
            {
                var indexColumns = props.Where(x => x.IsIndex).ToList();
                var singles = indexColumns.Where(x => x.IndexGroup == int.MinValue).ToList();
                var indexSetBuilder = new StringBuilder();
                foreach (var item in singles)
                {
                    indexSetBuilder.AppendLine($"builder.AddIndex(\"{item.Name}\",orderDesc:{item.IndexIsDesc.ToBoolKeyword()},name:{item.IndexName.ObjectToCsharp()});");
                }
                var grouping = indexColumns.Except(singles)
                    .GroupBy(x => x.IndexGroup);
                foreach (var item in grouping)
                {
                    var first = item.First();
                    var indexName = item.Where(x => !string.IsNullOrEmpty(x.IndexName)).FirstOrDefault();
                    indexSetBuilder.AppendLine($"builder.AddIndex(new System.String[]{{ {string.Join(",",item.Select(x=>$"\"{x.Name}\""))},orderDesc:{first.IndexIsDesc.ToBoolKeyword()},name:{indexName?.IndexName.ObjectToCsharp()});");
                }
                indexSet = indexSetBuilder.ToString();
            }

            var writeReadColumnResults = props.Select(x => x.WriteReadColumn("record")).ToList();
            var ordians = string.Join("\n", writeReadColumnResults.Select(x => x.OridinalCall+";"));
            var writes= string.Join(",\n", writeReadColumnResults.Select(x => x.WriteCall));
            var code = @$"
            #nullable enable
            {nameSpaceStart}
                using FastBIRe.Builders;

                {Consts.DebuggerStepThrough}
                {Consts.CompilerGenerated}
                {Consts.RecordToAttribute.WriteAttribute(fullName, className)}
                {visibility} class {className} : global::FastBIRe.IRecordToObject<{fullName}>,global::FastBIRe.Builders.ITableConfiger
                {{
                    [global::System.Runtime.CompilerServices.ModuleInitializer]
                    internal static void Init{className}()
                    {{
                        _ = Instance;
                    }}
                    public static readonly {className} Instance = new {className}();

                    private {className}()
                    {{
                        global::FastBIRe.RecordToObjectManager<{fullName}>.SetRecordToObject(this);
                    }}
                    public void Config(global::FastBIRe.Builders.ITableBuilder builder)
                    {{
                        {string.Join("\n", props.Select(x => x.WriteBuildColumn("builder")))}
                        {keySet}
                        {indexSet}
                    }}
                    public {fullName}{nullableEnd} To(global::System.Data.IDataRecord record)
                    {{
                        {ordians}
                        var obj = new {fullName}
                        {{
                            {writes}
                        }};
                        {string.Join("\n", afterCallMethods.Select(x => $"obj.{x.Symbol.Name}();"))}
                        return obj;
                    }}
                    public global::System.Collections.Generic.IList<{fullName}{nullableEnd}> ToList(global::System.Data.IDataReader reader)
                    {{
                        var res = new global::System.Collections.Generic.List<{fullName}{nullableEnd}>();
                        while(reader.Read())
                        {{
                            res.Add(To(reader));
                        }}
                        return res;
                    }}
                    public global::System.Collections.Generic.IEnumerable<{fullName}{nullableEnd}> Enumerable(global::System.Data.IDataReader reader)
                    {{
                        while(reader.Read())
                        {{
                            yield return To(reader);
                        }}
                    }}
                }}
            {nameSpaceEnd}
            #nullable restore
                ";
            
            code = Helpers.FormatCode(code);
            context.AddSource($"{className}.g.cs", code);
        }
        public string WriteReadColumn(string buildName)
        {
            return $"{buildName}.Column";
        }
        public string GetVisiblity(ISymbol symbol)
        {
            var attr = symbol.GetAttribute(Consts.GenerateModelAttribute.FullName)!;
            var isPublic = attr.GetByIndex<bool>(0);
            var visibility = GeneratorTransformResult<ISymbol>.GetAccessibilityString(Accessibility.Internal);
            if (isPublic)
            {
                visibility = GeneratorTransformResult<ISymbol>.GetAccessibilityString(Accessibility.Public);
            }
            return visibility;
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
