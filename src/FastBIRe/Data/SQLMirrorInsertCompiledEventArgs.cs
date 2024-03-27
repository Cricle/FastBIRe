namespace FastBIRe.Data
{
    public class DataMirrorCreatedInputEventArgs<TKey, TResult, TInput> : DataMirrorEventArgs<TKey, TResult, TInput>
    {
        public DataMirrorCreatedInputEventArgs(UndefinedDataMirrorCopy<TKey, TResult, TInput> mirrorCopy, TInput input) : base(mirrorCopy)
        {
            Input = input;
        }

        public TInput Input { get; }
    }
    public class DataMirrorEventArgs<TKey, TResult, TInput> : EventArgs
    {
        public DataMirrorEventArgs(UndefinedDataMirrorCopy<TKey, TResult, TInput> mirrorCopy)
        {
            MirrorCopy = mirrorCopy;
        }

        public UndefinedDataMirrorCopy<TKey, TResult, TInput> MirrorCopy { get; }
    }
    public class DataMirrorWritingEventArgs<TKey, TResult, TInput> : DataMirrorEventArgs<TKey, TResult, TInput>
    {
        public DataMirrorWritingEventArgs(UndefinedDataMirrorCopy<TKey, TResult, TInput> mirrorCopy, TInput input, bool storeResult, bool unbound) : base(mirrorCopy)
        {
            Input = input;
            StoreResult = storeResult;
            Unbound = unbound;
        }

        public TInput Input { get; }

        public bool StoreResult { get; }

        public bool Unbound { get; }
    }
    public class DataMirrorWritedEventArgs<TKey, TResult, TInput> : DataMirrorWritingEventArgs<TKey, TResult, TInput>
    {
        public DataMirrorWritedEventArgs(UndefinedDataMirrorCopy<TKey, TResult, TInput> mirrorCopy, TInput input, bool storeResult, bool unbound, TResult result) : base(mirrorCopy, input, storeResult, unbound)
        {
            Result = result;
        }

        public TResult Result { get; }
    }
}
