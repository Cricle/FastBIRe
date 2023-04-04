using DatabaseSchemaReader.Compare;

namespace FastBIRe
{
    public class CompareWithModifyResult
    {
        public CompareSchemas? Schemas { get; set; }

        public CompareWithModifyResultTypes Type { get; set; }

        public string? Execute()
        {
            return Schemas?.Execute();
        }
    }
}
