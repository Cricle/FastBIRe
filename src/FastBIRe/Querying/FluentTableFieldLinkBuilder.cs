using FastBIRe.Timing;

namespace FastBIRe.Querying
{
    public class FluentTableFieldLinkBuilder : List<ITableFieldLink>
    {
        public FluentTableFieldLinkBuilder(TableFieldLinkBuilder builder, FunctionMapper? functionMapper = null)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
            FunctionMapper = functionMapper ?? builder.FunctionMapper;
        }

        public TableFieldLinkBuilder Builder { get; }

        public FunctionMapper FunctionMapper { get; }

        public FluentTableFieldLinkBuilder Direct(string destFieldName, string sourceFieldName)
        {
            Add(Builder.Direct(destFieldName, sourceFieldName));
            return this;
        }
        public FluentTableFieldLinkBuilder Expand(string destFieldName, string column, Func<FunctionMapper, string> expandResultCreator)
        {
            return Expand(destFieldName, DefaultExpandResult.Expression(column, expandResultCreator(FunctionMapper)));
        }
        public FluentTableFieldLinkBuilder Expand(string destFieldName, Func<FunctionMapper, IExpandResult> expandResultCreator)
        {
            return Expand(destFieldName, expandResultCreator(FunctionMapper));
        }
        public FluentTableFieldLinkBuilder Expand(string destFieldName, IExpandResult expandResult)
        {
            Add(Builder.Expand(destFieldName, expandResult));
            return this;
        }
    }
}
