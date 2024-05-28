using DatabaseSchemaReader.DataSchema;
using System.Runtime.CompilerServices;

namespace FastBIRe
{
    public static partial class InterpolatedHelper
    {
        public static List<FormatResult> GetFormatResults(string format)
        {
            var res = new List<FormatResult>();
            var isIn = false;
            var startIndex = -1;
            var len = format.Length;
            var sp = format.AsSpan();
            for (int i = 0; i < len; i++)
            {
                var c = sp[i];
                if (c == '{')
                {
                    isIn = !isIn;
                    if (isIn)
                    {
                        startIndex = i;
                    }
                    continue;
                }
                if (c == '}')
                {
                    isIn = !isIn;
                    if (!isIn && startIndex != -1)
                    {
                        var block = sp.Slice(startIndex, i - startIndex + 1);
                        var colonIndex = block.IndexOf(':');
                        if (colonIndex == -1)
                        {
                            res.Add(new FormatResult(
                                format,
                                startIndex, i - startIndex + 1,
                                1, block.Length - 2,
                                -1, -1));
                        }
                        else
                        {
                            res.Add(new FormatResult(
                                 format,
                                 startIndex, i - startIndex + 1,
                                 startIndex + 1, colonIndex - 1,
                                 colonIndex + 1 + startIndex, block.Length - colonIndex - 2));
                        }
                    }
                    startIndex = -1;
                    continue;
                }
            }
            return res;
        }

        public static InterpolatedResult Parse(SqlType sqlType, FormattableString formattableString, string argPrefx = "p")
        {
            var escaper = sqlType.GetEscaper();
            var inputArgs = formattableString.GetArguments();
            var args = AllocArray<KeyValuePair<string, object?>>(inputArgs.Length);
            var formatArgs = AllocArray<string>(inputArgs.Length);
            for (int i = 0; i < inputArgs.Length; i++)
            {
                var name = $"@{argPrefx}{i}";
                formatArgs[i] = name;
                args[i] = new KeyValuePair<string, object?>(name, inputArgs[i]);
            }

            var sql = escaper.ReplaceParamterPrefixSql(string.Format(formattableString.Format, formatArgs), '@') ?? string.Empty;

            return new InterpolatedResult(sql, formattableString.Format, args, formattableString);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T[] AllocArray<T>(int size)
        {
            if (size == 0)
            {
                return Array.Empty<T>();
            }
#if NET6_0_OR_GREATER
            return GC.AllocateUninitializedArray<T>(size);
#else
            return new T[size];
#endif
        }
    }
}
