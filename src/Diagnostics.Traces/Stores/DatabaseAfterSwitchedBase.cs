namespace Diagnostics.Traces.Stores
{
    public abstract class DatabaseAfterSwitchedBase<TResult> : IUndefinedDatabaseAfterSwitched<TResult>
        where TResult : IDatabaseCreatedResult
    {
        protected DatabaseAfterSwitchedBase(IDeleteRules? deleteRules = null, IFileConversionProvider? fileConversionProvider = null)
        {
            DeleteRules = deleteRules;
            FileConversionProvider = fileConversionProvider;
        }
        protected virtual object? GetLocker(TResult result)
        {
            return result.Root;
        }

        protected virtual string? GetFilePath(TResult result)
        {
            return result.FilePath;
        }

        protected virtual Task BeginGzipAsync()
        {
            return Task.Delay(1000);
        }

        public event EventHandler<Exception>? ExceptionRaised;

        public IDeleteRules? DeleteRules { get; }

        public IFileConversionProvider? FileConversionProvider { get; }

        protected abstract Stream GetAfterStream(Stream stream);

        protected abstract string FailGetConvertsionPath(string filePath);

        public void AfterSwitched(TResult result)
        {
            var filePath = GetFilePath(result);
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }
            _ = Task.Factory.StartNew(async () =>
            {
                await BeginGzipAsync();
                var locker = GetLocker(result);
                if (locker != null)
                {
                    Monitor.Enter(locker);
                }
                try
                {
                    if (filePath != null && File.Exists(filePath))
                    {
                        var gzPath = FileConversionProvider?.ConvertPath(filePath) ?? FailGetConvertsionPath(filePath);
                        using (var raw = File.OpenRead(filePath))
                        using (var fs = File.Create(gzPath))
                        using (var gz = GetAfterStream(fs))
                        {
                            raw.CopyTo(gz);
                        }
                        File.Delete(filePath);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionRaised?.Invoke(this, ex);
                }
                finally
                {
                    if (locker != null)
                    {
                        Monitor.Exit(locker);
                    }
                    DeleteRules?.Raise();
                }
            });
        }
    }
}
