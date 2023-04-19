using DatabaseSchemaReader.Compare;

namespace FastBIRe
{
    public class CompareWithModifyResult
    {
        public CompareSchemas? Schemas { get; set; }

        public CompareWithModifyResultTypes Type { get; set; }

        public List<string> Execute()
        {
            if (Schemas == null)
            {
                return new List<string>(0);
            }
            var result = Schemas.ExecuteResult();
            return result.Select(x => x.Script).ToList();
        }
    }
}
