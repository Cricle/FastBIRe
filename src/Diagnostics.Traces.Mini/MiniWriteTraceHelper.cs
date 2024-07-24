using Diagnostics.Traces.Serialization;
using OpenTelemetry.Logs;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Diagnostics.Traces.Mini
{
    public class MiniWriteTraceHelper : IDisposable
    {
        private IMiniWriteSerializer miniWriteSerializer;

        public MiniWriteTraceHelper(IMiniWriteSerializer miniWriteSerializer)
        {
            this.miniWriteSerializer = miniWriteSerializer ?? throw new ArgumentNullException(nameof(miniWriteSerializer));
        }

        public IMiniWriteSerializer WriteSerializer => miniWriteSerializer;


        private void Write<T, TMode>(Action<DefaultWritableBuffer, T, TMode> action, T value, TMode mode)
            where TMode : struct, Enum
        {
            using (var writableBuffer = new DefaultWritableBuffer())
            {
                action(writableBuffer, value, mode);
                var sp = writableBuffer.BufferWriter.WrittenSpan;
                var needCompress = sp.Length <= 512;
                if (needCompress)
                {
                    var header = MiniDataHeader<TMode>.Create(sp, mode, TraceCompressMode.None);
                    WriteHeader(header);
                    miniWriteSerializer.Write(sp);
                }
                else
                {
                    using (var zstdResult=ZstdHelper.WriteZstd(sp))
                    {
                        var header = MiniDataHeader<TMode>.Create(zstdResult.Span, mode, TraceCompressMode.Zstd);
                        WriteHeader(header);
                        miniWriteSerializer.Write(zstdResult.Span);
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void WriteHeader<TMode>(in MiniDataHeader<TMode> header)
            where TMode : struct, Enum
        {
            byte* buffer = stackalloc byte[MiniDataHeader<TMode>.HeaderSize];
            Unsafe.Write(buffer, header);
            miniWriteSerializer.Write(new ReadOnlySpan<byte>(buffer, MiniDataHeader<TMode>.HeaderSize));
        }

        public void WriteLog(LogRecord log, SaveLogModes mode)
        {
            Write(static (ser, value, mode) => ser.WriteLog(value, mode), log, mode);
        }
        public void WriteActivity(Activity activity, SaveActivityModes mode)
        {
            Write(static (ser, value, mode) => ser.WriteActivity(value, mode), activity, mode);
        }
        public void WriteException(TraceExceptionInfo exception, SaveExceptionModes mode)
        {
            Write(static (ser, value, mode) => ser.WriteException(value, mode), exception, mode);
        }

        public void Dispose()
        {
            if (miniWriteSerializer is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
