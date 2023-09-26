namespace FastBIRe.Querying
{
    public partial class MergeQuerying
    {
        // Actualy I think the table alias should be easily recognizable
        public const string DefaultSourceTableAlias = "source";
        public const string DefaultDestTableAlias = "dest";
        public const string DefaultEffectTableAlias = "effect";

        public static readonly MergeQuerying Default = new MergeQuerying(DefaultSourceTableAlias, DefaultDestTableAlias, DefaultEffectTableAlias);

        public MergeQuerying(string sourceTableAlias, string destTableAlias, string effectTableAlias)
        {
            SourceTableAlias = sourceTableAlias;
            DestTableAlias = destTableAlias;
            EffectTableAlias = effectTableAlias;
        }

        public string SourceTableAlias { get; }

        public string DestTableAlias { get; }

        public string EffectTableAlias { get; }
    }
}
