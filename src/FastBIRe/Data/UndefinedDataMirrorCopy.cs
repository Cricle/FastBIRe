using System.Data;

namespace FastBIRe.Data
{
    public abstract class UndefinedDataMirrorCopy<TKey, TResult, TInput> : IMirrorCopy<TResult>
    {
        public const int DefaultSize = 400;

        protected UndefinedDataMirrorCopy(IDataReader dataReader)
            : this(dataReader, DefaultSize)
        {
        }
        protected UndefinedDataMirrorCopy(IDataReader dataReader, int batchSize)
        {
            if (batchSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(batchSize), "batchSize must > 0");
            }
            DataReader = dataReader;
            BatchSize = batchSize;
        }

        public int BatchSize { get; }

        public IDataReader DataReader { get; }

        public bool StoreWriteResult { get; set; }

        protected virtual IList<TResult> CreateResultStore()
        {
            return Array.Empty<TResult>();
        }
        protected virtual void CloseInput(TInput? input)
        {
            if (input is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        public async Task<IList<TResult>> CopyAsync(CancellationToken token = default)
        {
            var storeWriteResult = StoreWriteResult;
            var result = CreateResultStore();
            await OnCopyingAsync();
            var first = false;
            var input = CreateInput();
            var currentSize = 0;
            while (DataReader.Read())
            {
                if (!first)
                {
                    await OnFirstReadAsync();
                    first = true;
                }
                if (currentSize >= BatchSize)
                {
                    var res = await WriteAsync(input, storeWriteResult, false, token);
                    if (storeWriteResult)
                    {
                        result.Add(res);
                    }
                    CloseInput(input);
                    input = CreateInput();
                    currentSize = 0;
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                }
                else
                {
                    AppendRecord(input, DataReader, BatchSize == currentSize + 1);
                    currentSize++;
                }
            }
            if (currentSize != 0)
            {
                var res = await WriteAsync(input, storeWriteResult, true, token);
                if (storeWriteResult)
                {
                    result.Add(res);
                }
            }
            await OnCopyedAsync(result);
            return result;
        }
        protected virtual object? ConvertObject(object? input, int index)
        {
            if (input == DBNull.Value)
            {
                return null;
            }
            return input;
        }
        protected virtual Task OnFirstReadAsync()
        {
            return Task.CompletedTask;
        }
        protected virtual Task OnCopyingAsync()
        {
            return Task.CompletedTask;
        }
        protected virtual Task OnCopyedAsync(IList<TResult> results)
        {
            return Task.CompletedTask;
        }

        protected abstract TInput CreateInput();

        protected abstract void AppendRecord(TInput input, IDataReader reader, bool lastBatch);

        protected abstract Task<TResult> WriteAsync(TInput datas, bool storeWriteResult, bool unbounded, CancellationToken token);
    }

}
