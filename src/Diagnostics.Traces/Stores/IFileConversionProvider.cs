namespace Diagnostics.Traces.Stores
{
    public interface IFileConversionProvider
    {
        string ConvertPath(string filePath);
    }
}
