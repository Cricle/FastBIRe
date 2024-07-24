using OpenTelemetry.Logs;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Diagnostics.Traces.Serialization
{
    public static unsafe class MiniWriteSerializerExtensions
    {
        private const int IntSize = sizeof(int);

        private static void WriteContext(IWritableBuffer serializer, in ActivityContext context)
        {
            serializer.Write(context.TraceId.ToString());
            serializer.Write(context.SpanId.ToString());
            serializer.Write(context.TraceFlags);
            serializer.Write(context.TraceState);
            serializer.Write(context.IsRemote);
        }
        private static void WriteLinks(IWritableBuffer serializer, IEnumerable<ActivityLink> links)
        {
            var count = links.Count();
            serializer.Write(count);

            foreach (var item in links)
            {
                WriteContext(serializer, item.Context);
                WriteTags(serializer, item.Tags);
            }
        }
        private static void WriteEvents(IWritableBuffer serializer, IEnumerable<ActivityEvent> @event)
        {
            var count = @event.Count();
            serializer.Write(count);

            foreach (var item in @event)
            {
                serializer.Write(item.Name);
                serializer.Write(item.Timestamp.DateTime);
                WriteTags(serializer, item.Tags);
            }
        }
        private static void WriteTags(IWritableBuffer serializer, IEnumerable<KeyValuePair<string, object?>>? tags)
        {
            var count = 0;
            if (tags != null)
            {
                count = tags.Count();
            }
            serializer.Write(count);
            if (tags != null)
            {
                foreach (var item in tags)
                {
                    serializer.Write(item.Key);
                    serializer.Write(item.Value?.ToString());
                }
            }
        }
        private static void WriteTags(IWritableBuffer serializer, IEnumerable<KeyValuePair<string, string?>>? tags)
        {
            var count = 0;
            if (tags != null)
            {
                count = tags.Count();
            }
            serializer.Write(count);
            if (tags != null)
            {
                foreach (var item in tags)
                {
                    serializer.Write(item.Key);
                    serializer.Write(item.Value);
                }
            }
        }
        public static void WriteException(this IWritableBuffer serializer, in TraceExceptionInfo info, SaveExceptionModes mode)
        {
            if ((mode & SaveExceptionModes.TraceId) != 0)
            {
                serializer.Write(info.TraceId?.ToString());
            }
            if ((mode & SaveExceptionModes.SpanId) != 0)
            {
                serializer.Write(info.SpanId?.ToString());
            }
            if ((mode & SaveExceptionModes.CreateTime) != 0)
            {
                serializer.Write(info.CreateTime);
            }
            if ((mode & SaveExceptionModes.TypeName) != 0)
            {
                serializer.Write(info.Exception.GetType().FullName);
            }
            if ((mode & SaveExceptionModes.Message) != 0)
            {
                serializer.Write(info.Exception.Message);
            }
            if ((mode & SaveExceptionModes.HelpLink) != 0)
            {
                serializer.Write(info.Exception.HelpLink);
            }
            if ((mode & SaveExceptionModes.HResult) != 0)
            {
                serializer.Write(info.Exception.HResult);
            }
            if ((mode & SaveExceptionModes.StackTrace) != 0)
            {
                serializer.Write(info.Exception.StackTrace);
            }
            if ((mode & SaveExceptionModes.InnerException) != 0)
            {
                serializer.Write(info.Exception.InnerException?.ToString());
            }
        }
        public static void WriteActivity(this IWritableBuffer serializer, Activity activity, SaveActivityModes mode)
        {
            if ((mode & SaveActivityModes.Id) != 0)
            {
                serializer.Write(activity.Id);
            }
            if ((mode & SaveActivityModes.Status) != 0)
            {
                serializer.Write(activity.Status);

            }
            if ((mode & SaveActivityModes.StatusDescription) != 0)
            {
                serializer.Write(activity.StatusDescription);

            }
            if ((mode & SaveActivityModes.HasRemoteParent) != 0)
            {
                serializer.Write(activity.HasRemoteParent);

            }
            if ((mode & SaveActivityModes.Kind) != 0)
            {
                serializer.Write(activity.Kind);

            }
            if ((mode & SaveActivityModes.OperationName) != 0)
            {
                serializer.Write(activity.OperationName);

            }
            if ((mode & SaveActivityModes.DisplayName) != 0)
            {
                serializer.Write(activity.DisplayName);

            }
            if ((mode & SaveActivityModes.SourceName) != 0)
            {
                serializer.Write(activity.Source.Name);

            }
            if ((mode & SaveActivityModes.SourceVersion) != 0)
            {
                serializer.Write(activity.Source.Version);

            }
            if ((mode & SaveActivityModes.Duration) != 0)
            {
                serializer.Write(activity.Duration);

            }
            if ((mode & SaveActivityModes.StartTimeUtc) != 0)
            {
                serializer.Write(activity.StartTimeUtc);

            }
            if ((mode & SaveActivityModes.ParentId) != 0)
            {
                serializer.Write(activity.ParentId);

            }
            if ((mode & SaveActivityModes.RootId) != 0)
            {
                serializer.Write(activity.RootId);

            }
            if ((mode & SaveActivityModes.Tags) != 0)
            {
                WriteTags(serializer, activity.Tags);
            }
            if ((mode & SaveActivityModes.Events) != 0)
            {
                WriteEvents(serializer, activity.Events);
            }
            if ((mode & SaveActivityModes.Links) != 0)
            {
                WriteLinks(serializer, activity.Links);

            }
            if ((mode & SaveActivityModes.Baggage) != 0)
            {
                WriteTags(serializer, activity.Baggage);

            }
            if ((mode & SaveActivityModes.Context) != 0)
            {
                WriteContext(serializer, activity.Context);

            }
            if ((mode & SaveActivityModes.TraceStateString) != 0)
            {
                serializer.Write(activity.TraceStateString);

            }
            if ((mode & SaveActivityModes.SpanId) != 0)
            {
                serializer.Write(activity.SpanId.ToString());

            }
            if ((mode & SaveActivityModes.TraceId) != 0)
            {
                serializer.Write(activity.TraceId.ToString());

            }
            if ((mode & SaveActivityModes.Recorded) != 0)
            {
                serializer.Write(activity.Recorded);
            }
            if ((mode & SaveActivityModes.ActivityTraceFlags) != 0)
            {
                serializer.Write(activity.ActivityTraceFlags);

            }
            if ((mode & SaveActivityModes.ParentSpanId) != 0)
            {
                if (activity.ParentSpanId.Equals(default))
                {
                    serializer.Write(null);

                }
                else
                {
                    serializer.Write(activity.ParentSpanId.ToString());
                }
            }
        }

        public static void WriteLog(this IWritableBuffer serializer, LogRecord record, SaveLogModes mode)
        {
            if ((mode & SaveLogModes.Timestamp) != 0)
            {
                serializer.Write(record.Timestamp);
            }
            if ((mode & SaveLogModes.LogLevel) != 0)
            {
                serializer.Write(record.LogLevel);
            }
            if ((mode & SaveLogModes.CategoryName) != 0)
            {
                serializer.Write(record.CategoryName);
            }
            if ((mode & SaveLogModes.TraceId) != 0)
            {
                if ((record.TraceId.Equals(default)))
                {
                    Write(serializer, null);
                }
                else
                {
                    serializer.Write(record.TraceId.ToString());
                }
            }
            if ((mode & SaveLogModes.SpanId) != 0)
            {
                if ((record.SpanId.Equals(default)))
                {
                    Write(serializer, null);
                }
                else
                {
                    serializer.Write(record.SpanId.ToString());
                }
            }
            if ((mode & SaveLogModes.FormattedMessage) != 0)
            {
                serializer.Write(record.FormattedMessage);
            }
            if ((mode & SaveLogModes.Body) != 0)
            {
                serializer.Write(record.Body);
            }
        }

        public static unsafe void Write(this IWritableBuffer serializer, string? value)
        {
            byte* lengthBuffer = stackalloc byte[IntSize];

            var length = 0;
            if (value == null)
            {
                length = -1;
            }
            else if (value != string.Empty)
            {
                length = Encoding.UTF8.GetByteCount(value);
            }
            Write(serializer, length);
            if (!string.IsNullOrEmpty(value))
            {
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
                    fixed (byte* destPtr = sp)
                    {
                        //Error
                        var written = Encoding.UTF8.GetBytes((char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(value.AsSpan())),
                            value!.Length,
                            destPtr,
                            length);
                        serializer.Write(sp.Slice(0, written));
                    }
                }
                finally
                {
                    if (returnBytes != null)
                    {
                        ArrayPool<byte>.Shared.Return(returnBytes);
                    }
                }
            }
        }
        public static unsafe void Write<T>(this IWritableBuffer serializer, in T value)
            where T : unmanaged
        {
            byte* buffer = stackalloc byte[sizeof(T)];
            *(T*)buffer = value;
            serializer.Write(new ReadOnlySpan<byte>(buffer, sizeof(T)));
        }
    }
}
