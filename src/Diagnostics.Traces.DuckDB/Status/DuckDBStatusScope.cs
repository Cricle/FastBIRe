using Diagnostics.Generator.Core;
using Diagnostics.Traces.Status;
using System.Runtime.CompilerServices;
using ValueBuffer;

namespace Diagnostics.Traces.DuckDB.Status
{
    public class DuckDBStatusScope : StatusScopeBase
    {
        internal DuckDBStatusScope(string name, string tableName,DateTime time, StatusRemoveMode removeMode, BufferOperator<string> bufferOperator, IStatusStorageManager statusStorageManager)
            :base(time)
        {
            Key = name;
            Name = tableName;
            this.removeMode = removeMode;
            this.bufferOperator = bufferOperator;
            this.statusStorageManager = statusStorageManager;
            if (statusStorageManager.TryGetValue(Name, out var storage))
            {
                storage.AddScope(this);
            }
        }
        internal readonly IStatusStorageManager statusStorageManager;
        internal readonly StatusRemoveMode removeMode;
        internal readonly BufferOperator<string> bufferOperator;

        public override string Name { get; }

        public override string Key { get; }

        public override void Dispose()
        {
            if (!IsComplated)
            {
                OnComplate(StatusTypes.Unset);
            }
        }
        public override string ToString()
        {
            using var builder = new ValueStringBuilder();

            builder.Append($"{Name}({(IsComplated?"running":"done")})");
            builder.Append(Environment.NewLine);
            foreach (var item in logs)
            {
                builder.Append($"{item.Time:yyyy-MM-dd HH:mm:ss.fff}: {item.Value}");
            }
            builder.Append(Environment.NewLine);
            foreach (var item in status)
            {
                builder.Append($"{item.Time:yyyy-MM-dd HH:mm:ss.fff}: {item.Value}");
            }
            return builder.ToString();
        }
        protected override void OnComplate(StatusTypes types = StatusTypes.Unset)
        {
            if (statusStorageManager.TryGetValue(Name, out var storage))
            {
                storage.ComplatedScope(this, types);
            }
            if (removeMode == StatusRemoveMode.DropAll)
            {
                return;
            }
            if (removeMode == StatusRemoveMode.DropSucceed && types != StatusTypes.Fail)
            {
                return;
            }
            var builder = new ValueStringBuilder();
            try
            {
                builder.Append("INSERT INTO \"");
                builder.Append(Name);
                builder.Append("\" VALUES('");
                AppendTime(ref builder, StartTime);
                builder.Append("',");
                AppendTimePairValues(ref builder, logs);
                builder.Append(',');
                AppendTimePairValues(ref builder, status);
                builder.Append(",CURRENT_TIMESTAMP,");
                builder.Append(((int)types).ToString());
                builder.Append(')');
                var sql = builder.ToString();
                bufferOperator.Add(sql);
            }
            finally
            {
                builder.Dispose();

            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AppendTime(ref ValueStringBuilder builder, in DateTime dateTime)
        {
#if NET8_0_OR_GREATER
            Span<char> formatBuffer = stackalloc char[32];
            dateTime.TryFormat(formatBuffer, out var written, "yyyy-MM-dd HH:mm:ss.ffff");
            builder.Append(formatBuffer.Slice(0,written));
#else
            builder.Append(dateTime.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AppendTimePairValues(ref ValueStringBuilder builder, IEnumerable<TimePairValue> values)
        {
            builder.Append("MAP {");
            var isFirst = true;
            foreach (var item in values)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    builder.Append(',');
                }
                builder.Append('\'');
                AppendTime(ref builder, item.Time);
                builder.Append("':'");
                builder.Append(item.Value);
                builder.Append('\'');
            }
            builder.Append("}");
        }
    }
}
