using System.Text;

namespace FastBIRe.Building
{
    public class QueryMetadata : IQueryMetadata
    {
        public virtual IEnumerable<IQueryMetadata> GetChildren()
        {
            yield break;
        }

        public virtual void ToString(StringBuilder builder)
        {
            builder.Append(ToString());
        }
    }
}
