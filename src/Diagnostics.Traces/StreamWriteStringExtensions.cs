﻿using Diagnostics.Traces;
using System.Text;

namespace System.IO
{
    internal static class StreamWriteStringExtensions
    {
        public static ValueTask WriteStringAsync(this Stream stream, string value,CancellationToken token = default)
        {
            return WriteStringAsync(stream, value, Encoding.UTF8, token);
        }
        public static async ValueTask WriteStringAsync(this Stream stream, string value, Encoding encoding, CancellationToken token = default)
        {
            if (encoding is null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            using (var data = EncodingHelper.SharedEncoding(value, encoding))
            {
                await stream.WriteAsync(data.Buffers, 0, data.Count, token);
            }
        }
    }
}
