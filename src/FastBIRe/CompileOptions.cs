using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public record CompileOptions
    {
        public bool IncludeEffectJoin { get; set; }

        public string? EffectTable { get; set; }

        public DatabaseTable? SourceTable { get; set; }

        public DatabaseTable? DestTable { get; set; }
    }
}
