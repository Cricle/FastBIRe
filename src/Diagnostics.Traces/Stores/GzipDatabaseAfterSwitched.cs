using System.IO.Compression;

namespace Diagnostics.Traces.Stores
{
    public class GzipDatabaseAfterSwitched<TResult> : DatabaseAfterSwitchedBase<TResult>
        where TResult : IDatabaseCreatedResult
    {
        public CompressionLevel Level { get; }

        public GzipDatabaseAfterSwitched(CompressionLevel level, IDeleteRules? deleteRules = null, IFileConversionProvider? fileConversionProvider = null)
            : base(deleteRules, fileConversionProvider)
        {
            Level = level;
        }
        protected override Stream GetAfterStream(Stream stream)
        {
            return new GZipStream(stream, Level);
        }
        protected override string FailGetConvertsionPath(string filePath)
        {
            return filePath + ".gz";
        }
    }
}
