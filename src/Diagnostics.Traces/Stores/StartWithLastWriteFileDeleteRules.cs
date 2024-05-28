namespace Diagnostics.Traces.Stores
{
    public class StartWithLastWriteFileDeleteRules : IDeleteRules
    {
        public StartWithLastWriteFileDeleteRules(string path, int keepFileCount, string searchPattern = "*")
        {
            Path = path;
            KeepFileCount = keepFileCount;

            if (keepFileCount <= 1)
            {
                throw new ArgumentOutOfRangeException($"The keepFileCount is {keepFileCount}, must more than 1");
            }
            SearchPattern = searchPattern ?? throw new ArgumentNullException(nameof(searchPattern));
        }

        public string Path { get; }

        public int KeepFileCount { get; }

        public string SearchPattern { get; }

        public event EventHandler<Exception>? ExceptionRaised;

        public void Raise()
        {
            _ = Task.Factory.StartNew(() =>
            {
                try
                {
                    foreach (var item in Directory.EnumerateFiles(Path, SearchPattern, SearchOption.AllDirectories).OrderByDescending(x => File.GetLastWriteTime(x)).Skip(KeepFileCount))
                    {
                        try
                        {
                            File.Delete(item);
                        }
                        catch (Exception ex)
                        {
                            ExceptionRaised?.Invoke(this, ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExceptionRaised?.Invoke(this, ex);
                }
            });
        }
    }
}
