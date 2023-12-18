using System.Data.Common;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FastBIRe
{
    public partial class DefaultScriptExecuter
    {
        private static readonly IReadOnlyList<MethodBase> Methods = typeof(DefaultScriptExecuter).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(x => !x.IsSpecialName && x.DeclaringType == typeof(DefaultScriptExecuter))
            .ToArray();

        private static readonly double TickFrequency = (double)TimeSpan.TicksPerSecond / Stopwatch.Frequency;

        private static readonly Assembly Assembly = typeof(DefaultScriptExecuter).Assembly;

        public static StackFrame? GetSourceFrame(StackTrace stackTrace)
        {
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                if (frame != null && frame.HasSource() && frame.HasMethod())
                {
                    var method = frame.GetMethod();
                    if (method != null &&
                        method.DeclaringType != null &&
                        method.DeclaringType.Assembly != Assembly)
                    {
                        return frame;
                    }
                }
            }
            return null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static TimeSpan GetElapsedTime(long startingTimestamp)
        {
            return new TimeSpan((long)((Stopwatch.GetTimestamp() - startingTimestamp) * TickFrequency));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static TimeSpan GetElapsedTime(long startingTimestamp,long endTimestamp)
        {
            return new TimeSpan((long)((endTimestamp - startingTimestamp) * TickFrequency));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsExecuterMethod(MethodBase method)
        {
            return Methods.Contains(method);
        }

        public const int DefaultCommandTimeout = 60;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string SqlParameterConversion(string sql)
        {
            if (EnableSqlParameterConversion)
            {
                return Escaper.ReplaceParamterPrefixSql(sql, SqlParameterPrefix) ?? sql;
            }
            return sql;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string SqlQutoConversion(string sql)
        {
            if (EnableSqlQutoConversion)
            {
                return Escaper.ReplaceQutoSql(sql, QutoStart, QutoEnd) ?? sql;
            }
            return sql;
        }
        private StackTrace? GetStackTrace()
        {
            if (CaptureStackTrace)
            {
                return new StackTrace(StackTraceNeedFileInfo);
            }
            return null;
        }
        private static DbType GetDbType(object? input)
        {
            if (input == null)
            {
                return DbType.String;
            }

            var typeCode = Type.GetTypeCode(input.GetType());

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return DbType.Boolean;
                case TypeCode.Byte:
                    return DbType.Byte;
                case TypeCode.SByte:
                    return DbType.SByte;
                case TypeCode.Int16:
                    return DbType.Int16;
                case TypeCode.Char:
                case TypeCode.Int32:
                    return DbType.Int32;
                case TypeCode.Int64:
                    return DbType.Int64;
                case TypeCode.UInt16:
                    return DbType.UInt16;
                case TypeCode.UInt32:
                    return DbType.UInt32;
                case TypeCode.UInt64:
                    return DbType.UInt64;
                case TypeCode.Single:
                    return DbType.Single;
                case TypeCode.Double:
                    return DbType.Double;
                case TypeCode.Decimal:
                    return DbType.Decimal;
                case TypeCode.DateTime:
                    return DbType.DateTime;
                case TypeCode.DBNull:
                case TypeCode.Object:
                case TypeCode.String:
                case TypeCode.Empty:
                default:
                    return DbType.String;
            }
        }

        public static bool IsEmptyScript(string script)
        {
            return string.IsNullOrWhiteSpace(script) || AllLineStartWith(script, "--");
        }

        public static bool AllLineStartWith(string script, string startWith)
        {
            var startIndex = 0;
            var length = script.Length;
            for (int i = 0; i < length; i++)
            {
                if (script[i] == '\n')
                {
                    if (i == startIndex || !script.AsSpan(startIndex, i - 1 - startIndex).TrimStart().StartsWith(startWith.AsSpan()))
                    {
                        return false;
                    }
                    startIndex = i - 1;
                }
            }
            if (startIndex != length)
            {
                return script.AsSpan(startIndex).TrimStart().StartsWith(startWith.AsSpan());
            }
            return true;
        }
    }
}
