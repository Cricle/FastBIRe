using System.Text;

namespace FastBIRe.Building
{
    public static class QueryMetadataExtensions
    {
        public static IEnumerable<T> Find<T>(this IQueryMetadata metadata)
            where T : IQueryMetadata
        {
            if (metadata is T t)
            {
                yield return t;
            }
            foreach (var item in metadata.GetChildren())
            {
                foreach (var sub in Find<T>(item))
                {
                    yield return sub;
                }
            }
        }
    }
    public interface IQueryMetadata
    {
        string? ToString();

        void ToString(StringBuilder builder);

        IEnumerable<IQueryMetadata> GetChildren();
    }
}
