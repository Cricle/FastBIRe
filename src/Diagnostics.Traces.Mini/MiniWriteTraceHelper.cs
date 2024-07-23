using Diagnostics.Traces.Serialization;
using OpenTelemetry.Logs;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ZstdSharp;

namespace Diagnostics.Traces.Mini
{
    public class MiniWriteTraceHelper : IDisposable
    {
        private IMiniWriteSerializer miniWriteSerializer;
        private readonly Compressor compressor;

        public MiniWriteTraceHelper(IMiniWriteSerializer miniWriteSerializer)
        {
            this.miniWriteSerializer = miniWriteSerializer ?? throw new ArgumentNullException(nameof(miniWriteSerializer));
            compressor = new Compressor();
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
                    var header = MiniSerializeHeader<TMode>.Create(sp, mode, TraceCompressMode.None);
                    WriteHeader(header);
                    miniWriteSerializer.Write(sp);
                }
                else
                {
                    var len = Compressor.GetCompressBound(sp.Length);
                    var buffer = ArrayPool<byte>.Shared.Rent(len);
                    try
                    {
                        var writted = compressor.Wrap(sp, buffer);
                        var needWrite = buffer.AsSpan(0, writted);
                        var header = MiniSerializeHeader<TMode>.Create(needWrite, mode, TraceCompressMode.Zstd);
                        WriteHeader(header);
                        miniWriteSerializer.Write(needWrite);
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(buffer);
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void WriteHeader<TMode>(in MiniSerializeHeader<TMode> header)
            where TMode : struct, Enum
        {
            byte* buffer = stackalloc byte[MiniSerializeHeader<TMode>.HeaderSize];
            Unsafe.Write(buffer, header);
            miniWriteSerializer.Write(new ReadOnlySpan<byte>(buffer, MiniSerializeHeader<TMode>.HeaderSize));
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
            compressor.Dispose();
            if (miniWriteSerializer is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
