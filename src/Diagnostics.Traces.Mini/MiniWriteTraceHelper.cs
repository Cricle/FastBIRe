using Diagnostics.Traces.Serialization;
using OpenTelemetry.Logs;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Diagnostics.Traces.Mini
{
    public class MiniWriteTraceHelper
    {
        private IMiniWriteSerializer miniWriteSerializer;

        public MiniWriteTraceHelper(IMiniWriteSerializer miniWriteSerializer)
        {
            this.miniWriteSerializer = miniWriteSerializer ?? throw new ArgumentNullException(nameof(miniWriteSerializer));
        }

        public IMiniWriteSerializer WriteSerializer => miniWriteSerializer;

        private void Write<T, TMode>(Action<BufferMiniWriteSerializer, T, TMode> action, T value, TMode mode)
            where TMode : struct, Enum
        {
            using (var buffer = new BufferMiniWriteSerializer())
            {
                action(buffer, value, mode);
                var sp = buffer.Writer.WrittenSpan;
                var header = MiniSerializeHeader<TMode>.Create(sp, mode);
                WriteHeader(header);
                miniWriteSerializer.Write(sp);
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
    }
}
