using DatabaseSchemaReader;

namespace FastBIRe
{
    public class ModeHelperGroup<TModeRequest> : List<IModeHelper<TModeRequest>>, IModeHelper<TModeRequest>
    {
        public void Apply(DatabaseReader reader, TModeRequest request)
        {
            foreach (var item in this)
            {
                item.Apply(reader, request);
            }
        }
    }
}
