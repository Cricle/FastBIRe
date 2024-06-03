using System.Text;

namespace Diagnostics.Traces
{
    internal static class StreamWriteStringExtensions
    {
        public static ValueTask WriteStringAsync(this Stream stream, string value, CancellationToken token = default)
        {
            return stream.WriteStringAsync(value, Encoding.UTF8, token);
        }
        public static void WriteString(this Stream stream, string value)
        {
            stream.WriteString(value, Encoding.UTF8);
        }
        public static async ValueTask WriteStringAsync(this Stream stream, string value, Encoding encoding, CancellationToken token = default)
        {
            if (encoding is null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            using (var data = EncodingHelper.SharedEncoding(value, encoding))
            {
                await stream.WriteAsync(data.Buffers, 0, data.Count, token).ConfigureAwait(false);
            }
        }
        public static void WriteString(this Stream stream, string value, Encoding encoding, CancellationToken token = default)
        {
            if (encoding is null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            using (var data = EncodingHelper.SharedEncoding(value, encoding))
            {
                stream.Write(data.Buffers, 0, data.Count);
            }
        }
    }
}
