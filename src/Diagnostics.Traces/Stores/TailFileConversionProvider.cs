namespace Diagnostics.Traces.Stores
{
    public class TailFileConversionProvider : IFileConversionProvider
    {
        public TailFileConversionProvider(string tail)
        {
            Tail = tail;
        }

        public string Tail { get; }

        public string ConvertPath(string filePath)
        {
            return filePath + Tail;
        }
    }
}
