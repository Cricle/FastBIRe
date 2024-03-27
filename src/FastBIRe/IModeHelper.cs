using DatabaseSchemaReader;

namespace FastBIRe
{
    public interface IModeHelper<in TModeRequest>
    {
        void Apply(DatabaseReader reader, TModeRequest request);
    }
}
