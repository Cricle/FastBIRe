using Diagnostics.Traces.Stores;
using ZstdSharp;

namespace Diagnostics.Traces.Mini
{
    public class ZstdDatabaseAfterSwitched<TResult> : DatabaseAfterSwitchedBase<TResult>
        where TResult : IDatabaseCreatedResult
    {
        public int Level { get; }

        public ZstdDatabaseAfterSwitched(int level = 0, IDeleteRules? deleteRules = null, IFileConversionProvider? fileConversionProvider = null)
            : base(deleteRules, fileConversionProvider)
        {
            Level = level;
        }
        protected override Stream GetAfterStream(Stream stream)
        {
            return new CompressionStream(stream, Level, leaveOpen: false);
        }
        protected override string FailGetConvertsionPath(string filePath)
        {
            return filePath + ".zstd";
        }
    }
}
