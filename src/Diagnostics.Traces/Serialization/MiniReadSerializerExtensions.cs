using Diagnostics.Traces.Models;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Diagnostics.Traces.Serialization
{
    public static unsafe class MiniReadSerializerExtensions
    {
        private const int IntSize = sizeof(int);
        private static ActivityLinkContextEntity ReadContext(IMiniReadSerializer serializer)
        {
            return new ActivityLinkContextEntity
            {
                TraceId=ReadString(serializer),
                SpanId=ReadString(serializer),
                TraceFlags=(ActivityTraceFlags)Read<int>(serializer),
                TraceState=ReadString(serializer),
                IsRemote=Read<bool>(serializer)
            };
        }
        private static Dictionary<string,string?>? ReadTags(IMiniReadSerializer serializer)
        {
            var count = Read<int>(serializer);
            if (count==-1)
            {
                return null;
            }
            if (count==0)
            {
                return new Dictionary<string, string?>(0);
            }
            var map = new Dictionary<string, string?>();
            for (int i = 0; i < count; i++)
            {
                map[ReadString(serializer)!] = ReadString(serializer);
            }
            return map;
        }
        private static List<ActivityEventEntity> ReadEvents(IMiniReadSerializer serializer)
        {
            var count = Read<int>(serializer);
            if (count==0)
            {
                return new List<ActivityEventEntity>(0);
            }
            var res = new List<ActivityEventEntity>(count);

            for (int i = 0; i < count; i++)
            {
                res.Add(new ActivityEventEntity
                {
                    Name = ReadString(serializer),
                    Timestamp = Read<DateTime>(serializer),
                    Tags = ReadTags(serializer)
                });
            }

            return res;
        }
        public static ExceptionEntity ReadException(this IMiniReadSerializer serializer, SaveExceptionModes mode)
        {
            var entity = new ExceptionEntity();
            if ((mode & SaveExceptionModes.TraceId) != 0)
            {
                entity.TraceId = ReadString(serializer);
            }
            if ((mode & SaveExceptionModes.SpanId) != 0)
            {
                entity.SpanId= ReadString(serializer);
            }
            if ((mode & SaveExceptionModes.CreateTime) != 0)
            {
                entity.CreateTime = Read<DateTime>(serializer);
            }
            if ((mode & SaveExceptionModes.TypeName) != 0)
            {
                entity.TypeName = ReadString(serializer);
            }
            if ((mode & SaveExceptionModes.Message) != 0)
            {
                entity.Message = ReadString(serializer);
            }
            if ((mode & SaveExceptionModes.HelpLink) != 0)
            {
                entity.HelpLink = ReadString(serializer);
            }
            if ((mode & SaveExceptionModes.HResult) != 0)
            {
                entity.HResult = Read<int>(serializer);
            }
            if ((mode & SaveExceptionModes.StackTrace) != 0)
            {
                entity.StackTrace = ReadString(serializer);
            }
            if ((mode & SaveExceptionModes.InnerException) != 0)
            {
                entity.InnerException = ReadString(serializer);
            }
            return entity;
        }
        public static AcvtityEntity ReadActivity(this IMiniReadSerializer serializer, SaveActivityModes mode)
        {
            var entity = new AcvtityEntity();
            if ((mode & SaveActivityModes.Id) != 0)
            {
                entity.Id = ReadString(serializer);
            }
            if ((mode & SaveActivityModes.Status) != 0)
            {
                entity.Status = Read<ActivityStatusCode>(serializer);
            }
            if ((mode & SaveActivityModes.StatusDescription) != 0)
            {
                entity.StatusDescription = ReadString(serializer);
            }
            if ((mode & SaveActivityModes.HasRemoteParent) != 0)
            {
                entity.HasRemoteParent = Read<bool>(serializer);
            }
            if ((mode & SaveActivityModes.Kind) != 0)
            {
                entity.Kind= Read<ActivityKind>(serializer);
            }
            if ((mode & SaveActivityModes.OperationName) != 0)
            {
                entity.OperationName = ReadString(serializer);
            }
            if ((mode & SaveActivityModes.DisplayName) != 0)
            {
                entity.DisplayName = ReadString(serializer);
            }
            if ((mode & SaveActivityModes.SourceName) != 0)
            {
                entity.DisplayName = ReadString(serializer);
            }
            if ((mode & SaveActivityModes.SourceVersion) != 0)
            {
                entity.SourceVersion = ReadString(serializer);
            }
            if ((mode & SaveActivityModes.Duration) != 0)
            {
                entity.Duration = Read<double>(serializer);
            }
            if ((mode & SaveActivityModes.StartTimeUtc) != 0)
            {
                entity.StartTimeUtc = Read<DateTime>(serializer);
            }
            if ((mode & SaveActivityModes.ParentId) != 0)
            {
                entity.ParentId = ReadString(serializer);
            }
            if ((mode & SaveActivityModes.RootId) != 0)
            {
                entity.RootId = ReadString(serializer);
            }
            if ((mode & SaveActivityModes.Tags) != 0)
            {
                entity.Tags = ReadTags(serializer);
            }
            if ((mode & SaveActivityModes.Events) != 0)
            {
                entity.Events = ReadEvents(serializer);
            }
            if ((mode & SaveActivityModes.Links) != 0)
            {
                entity.Links = ReadLinks(serializer);
            }
            if ((mode & SaveActivityModes.Baggage) != 0)
            {
                entity.Baggage = ReadTags(serializer);
            }
            if ((mode & SaveActivityModes.Context) != 0)
            {
                entity.Context = ReadContext(serializer);
            }
            if ((mode & SaveActivityModes.TraceStateString) != 0)
            {
                entity.TraceStateString = ReadString(serializer);
            }
            if ((mode & SaveActivityModes.SpanId) != 0)
            {
                entity.SpanId = ReadString(serializer);
            }
            if ((mode & SaveActivityModes.TraceId) != 0)
            {
                entity.TraceId = ReadString(serializer);
            }
            if ((mode & SaveActivityModes.Recorded) != 0)
            {
                entity.Recorded = Read<bool>(serializer);
            }
            if ((mode & SaveActivityModes.ActivityTraceFlags) != 0)
            {
                entity.ActivityTraceFlags = Read<ActivityTraceFlags>(serializer);
            }
            if ((mode & SaveActivityModes.ParentSpanId) != 0)
            {
                entity.ParentSpanId = ReadString(serializer);
            }
            return entity;
        }

        public static LogEntity ReadLog(this IMiniReadSerializer serializer,SaveLogModes mode)
        {
            var log = new LogEntity();
            if ((mode & SaveLogModes.Timestamp) != 0)
            {
                log.Timestamp = Read<DateTime>(serializer);
            }
            if ((mode & SaveLogModes.LogLevel) != 0)
            {
                log.LogLevel = Read<LogLevel>(serializer);
            }
            if ((mode & SaveLogModes.CategoryName) != 0)
            {
                log.CategoryName= ReadString(serializer);
            }
            if ((mode & SaveLogModes.TraceId) != 0)
            {
                log.TraceId = ReadString(serializer);
            }
            if ((mode & SaveLogModes.SpanId) != 0)
            {
                log.SpanId = ReadString(serializer);

            }
            if ((mode & SaveLogModes.FormattedMessage) != 0)
            {
                log.FormattedMessage = ReadString(serializer);

            }
            if ((mode & SaveLogModes.Body) != 0)
            {
                log.Body = ReadString(serializer);
            }
            return log;
        }

        private static List<ActivityLinkEntity> ReadLinks(IMiniReadSerializer serializer)
        {
            var count = Read<int>(serializer);
            var res = new List<ActivityLinkEntity>(count);
            for (int i = 0; i < count; i++)
            {
                var entity = new ActivityLinkEntity
                {
                    Context = ReadContext(serializer),
                    Tags = ReadTags(serializer)
                };
            }
            return res;
        }
        public static unsafe string? ReadString(this IMiniReadSerializer serializer)
        {
            var length=Read<int>(serializer);
            if (length == -1)
            {
                return null;
            }
            if (length==0)
            {
                return string.Empty;
            }
            var needShared = length > 4096;
            byte[]? returnBytes = null;
            Span<byte> sp;
            if (needShared)
            {
                returnBytes = ArrayPool<byte>.Shared.Rent(length);
                sp = returnBytes.AsSpan(0, length);
            }
            else
            {
#pragma warning disable CS9081
                sp = stackalloc byte[length];
#pragma warning restore
            }
            try
            {
                serializer.Read(sp);
                return Encoding.UTF8.GetString((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(sp)), sp.Length);
            }
            finally
            {
                if (returnBytes != null)
                {
                    ArrayPool<byte>.Shared.Return(returnBytes);
                }

            }
        }

        public static unsafe T Read<T>(this IMiniReadSerializer serializer)
            where T : unmanaged
        {
            byte* buffer = stackalloc byte[sizeof(T)];
            serializer.Read(new Span<byte>(buffer, sizeof(T)));
            return *(T*)buffer;
        }
    }
}
