using Diagnostics.Traces.Models;
using Diagnostics.Traces.Serialization;
using K4os.Hash.xxHash;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Diagnostics.Traces.Mini
{
    public class MiniReadTraceHelper : IMiniReadSerializer, IDisposable
    {
        private IMiniReadSerializer miniReadSerializer;

        public MiniReadTraceHelper(IMiniReadSerializer miniReadSerializer)
        {
            this.miniReadSerializer = miniReadSerializer;
        }

        public IMiniReadSerializer MiniReadSerializer => miniReadSerializer;

        public bool CanSeek => miniReadSerializer.CanSeek;

        public unsafe MiniSerializeHeader<TMode>? ReadHeader<TMode>()
            where TMode : struct, Enum
        {
            var canReadResult = miniReadSerializer.CanRead(MiniSerializeHeader<TMode>.HeaderSize);
            if (!canReadResult)
            {
                return null;
            }
            byte* headerBuffer = stackalloc byte[MiniSerializeHeader<TMode>.HeaderSize];
            miniReadSerializer.Read(new Span<byte>(headerBuffer, MiniSerializeHeader<TMode>.HeaderSize));
            return Unsafe.Read<MiniSerializeHeader<TMode>>(headerBuffer);
        }

        private unsafe MiniReadResult<T> Read<T, TMode>(Func<ConstMiniReadSerializer, TMode, T> func)
            where TMode : struct, Enum
        {
            var header = ReadHeader<TMode>();
            if (header == null)
            {
                return new MiniReadResult<T>(MiniReadResultTypes.CanNotReadHeader);
            }

            if (!miniReadSerializer.CanRead(header.Value.Size))
            {
                return new MiniReadResult<T>(MiniReadResultTypes.CanNotReadBody);
            }

            byte[]? returnBuffer = null;
            Span<byte> buffer;
            if (header.Value.Size >= 2048)
            {
                returnBuffer = ArrayPool<byte>.Shared.Rent(header.Value.Size);
                buffer = returnBuffer.AsSpan(0, header.Value.Size);
            }
            else
            {
#pragma warning disable CS9081
                buffer = stackalloc byte[header.Value.Size];
#pragma warning restore CS9081
            }
            miniReadSerializer.Read(buffer);

            var inputHash = XXH32.DigestOf(buffer);
            if (inputHash != header.Value.Hash)
            {
                return new MiniReadResult<T>(MiniReadResultTypes.HashError);
            }

            var constReader = new ConstMiniReadSerializer((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), buffer.Length);
            var entity = func(constReader, header.Value.Mode);
            return new MiniReadResult<T>(entity, MiniReadResultTypes.Succeed);
        }
        public unsafe TraceHeader? ReadHead()
        {
            if (!miniReadSerializer.CanRead(TraceHeader.Size))
            {
                return null;
            }
            byte* buffer = stackalloc byte[TraceHeader.Size];
            miniReadSerializer.Read(new Span<byte>(buffer, TraceHeader.Size));
            return Unsafe.Read<TraceHeader>(buffer);
        }
        public MiniReadResult<LogEntity> ReadLog()
        {
            return Read<LogEntity, SaveLogModes>(static (ser, mode) => ser.ReadLog(mode));
        }
        public MiniReadResult<AcvtityEntity> ReadActivity()
        {
            return Read<AcvtityEntity, SaveActivityModes>(static (ser, mode) => ser.ReadActivity(mode));
        }
        public MiniReadResult<ExceptionEntity> ReadException()
        {
            return Read<ExceptionEntity, SaveExceptionModes>(static (ser, mode) => ser.ReadException(mode));
        }

        public void Dispose()
        {
            if (miniReadSerializer is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        public bool CanRead(int length)
        {
            return miniReadSerializer.CanRead(length);
        }

        public void Read(Span<byte> buffer)
        {
            miniReadSerializer.Read(buffer);
        }
    }
}
