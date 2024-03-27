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

        public IDataCapturer? DataCapturer { get; set; }

        public event EventHandler<DataMirrorEventArgs<TKey, TResult, TInput>>? Starting;
        public event EventHandler<DataMirrorEventArgs<TKey, TResult, TInput>>? Complated;
        public event EventHandler<DataMirrorEventArgs<TKey, TResult, TInput>>? Firsted;
        public event EventHandler<DataMirrorCreatedInputEventArgs<TKey, TResult, TInput>>? CreatedInput;
        public event EventHandler<DataMirrorWritingEventArgs<TKey, TResult, TInput>>? Writing;
        public event EventHandler<DataMirrorWritedEventArgs<TKey, TResult, TInput>>? Writed;

        protected virtual IList<TResult> CreateResultStore()
        {
            if (StoreWriteResult)
            {
                return new List<TResult>();
            }
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
            Starting?.Invoke(this, new DataMirrorEventArgs<TKey, TResult, TInput>(this));
            var storeWriteResult = StoreWriteResult;
            var result = CreateResultStore();
            await OnCopyingAsync();
            var first = false;
            var input = CreateInput();
            var currentSize = 0;
            while (DataReader.Read())
            {
                DataCapturer?.Capture(DataReader);
                if (!first)
                {
                    await OnFirstReadAsync();
                    Firsted?.Invoke(this, new DataMirrorEventArgs<TKey, TResult, TInput>(this));
                    first = true;
                }
                if (currentSize >= BatchSize)
                {
                    Writing?.Invoke(this, new DataMirrorWritingEventArgs<TKey, TResult, TInput>(this, input, storeWriteResult, false));
                    var res = await WriteAsync(input, storeWriteResult, false, token);
                    Writed?.Invoke(this, new DataMirrorWritedEventArgs<TKey, TResult, TInput>(this, input, storeWriteResult, false, res));
                    if (storeWriteResult)
                    {
                        result.Add(res);
                    }
                    CloseInput(input);
                    input = CreateInput();
                    CreatedInput?.Invoke(this, new DataMirrorCreatedInputEventArgs<TKey, TResult, TInput>(this, input));
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
                Writing?.Invoke(this, new DataMirrorWritingEventArgs<TKey, TResult, TInput>(this, input, storeWriteResult, true));
                var res = await WriteAsync(input, storeWriteResult, true, token);
                Writed?.Invoke(this, new DataMirrorWritedEventArgs<TKey, TResult, TInput>(this, input, storeWriteResult, false, res));
                if (storeWriteResult)
                {
                    result.Add(res);
                }
            }
            await OnCopyedAsync(result);
            Complated?.Invoke(this, new DataMirrorEventArgs<TKey, TResult, TInput>(this));
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
