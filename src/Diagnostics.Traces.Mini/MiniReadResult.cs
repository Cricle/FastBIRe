namespace Diagnostics.Traces.Mini
{
    public readonly struct MiniReadResult<TResult>
    {
        public readonly TResult? Result;

        public readonly MiniReadResultTypes ResultType;

        public MiniReadResult(MiniReadResultTypes resultType) : this(default,resultType)
        {
            ResultType = resultType;
        }

        public MiniReadResult(TResult? result, MiniReadResultTypes resultType)
        {
            Result = result;
            ResultType = resultType;
        }
    }
}
