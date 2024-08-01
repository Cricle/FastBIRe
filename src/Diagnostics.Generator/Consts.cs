using System.Data;
using System.Diagnostics;

namespace Diagnostics.Generator
{
    internal static class Consts
    {
        public const string Name = "Diagnostics";

        public static readonly string Version = typeof(Consts).Assembly.GetName().Version.ToString();

        public static readonly string CompilerGenerated = "[global::System.Runtime.CompilerServices.CompilerGenerated]";

        public const string DebuggerStepThrough = "[global::System.Diagnostics.DebuggerStepThrough]";

        public static readonly string GenerateCode = $"[global::System.CodeDom.Compiler.GeneratedCode(\"{Name}\",\"{Version}\")]";

        public static class EventSourceGenerateAttribute
        {
            public const string FullName = "Diagnostics.Generator.Core.Annotations.EventSourceGenerateAttribute";

            public const string IncludeInterface = "IncludeInterface";

            public const string InterfaceVisilbility = "InterfaceAccessibility";

            public const string GenerateSingleton = "GenerateSingleton";

            public const string UseIsEnable = "UseIsEnable";
        }
        public static class EventAttribute
        {
            public const string FullName = "System.Diagnostics.Tracing.EventAttribute";
        }
        public static class RelatedActivityIdAttribute
        {
            public const string FullName = "Diagnostics.Generator.Core.Annotations.RelatedActivityIdAttribute";
        }
        public static class CounterAttribute
        {
            public const string FullName = "Diagnostics.Generator.Core.Annotations.CounterAttribute";

            public const string DisplayRateTimeScaleMs = "DisplayRateTimeScaleMs";

            public const string DisplayName = "DisplayName";

            public const string DisplayUnits = "DisplayUnits";
        }
        public static class ArrayOptionsAttribute
        {
            public const string FullName = "Diagnostics.Generator.Core.Annotations.ArrayOptionsAttribute";
        }
        public static class CounterMappingAttribute
        {
            public const string FullName = "Diagnostics.Generator.Core.Annotations.CounterMappingAttribute";

            public const string ForProviders = "ForProviders";

            public const string ForAnysProviders = "ForAnysProviders";

            public const string WithInterval = "WithInterval";

            public const string WithCreator = "WithCreator";

            public const string CreatorHasInstance = "CreatorHasInstance";
        }
        public static class CounterItemAttribute
        {
            public const string FullName = "Diagnostics.Generator.Core.Annotations.CounterItemAttribute";

            public const string EventName = "EventName";
        }
        public static class EventPipeProviderAttribute
        {
            public const string FullName = "Diagnostics.Helpers.Annotations.EventPipeProviderAttribute";

            public const string Name = "Name";

            public const string Level = "Level";

            public const string Keywords = "Keywords";

            public const string Arguments = "Arguments";
        }
        public static class MapToActivityAttribute
        {
            public const string FullName = "Diagnostics.Generator.Core.Annotations.MapToActivityAttribute";

            public const string ActivityClassType = "ActivityClassType";

            public const string WithEventSourceCall = "WithEventSourceCall";

            public const string CallEventAtEnd = "CallEventAtEnd";

            public const string GenerateWithLog = "GenerateWithLog";
        }
        public static class ActivityIgnoreAttribute
        {
            public const string FullName = "Diagnostics.Generator.Core.Annotations.ActivityIgnoreAttribute";
        }
        public static class ActivityTagAttribute
        {
            public const string FullName = "Diagnostics.Generator.Core.Annotations.ActivityTagAttribute";

            public const string Name = "Name";

            public const string Expression = "Expression";

            public const string IsSet = "IsSet";

            public const string IsAdd = "IsAdd";
        }
        public static class ActivityNoEventAttribute
        {
            public const string FullName = "Diagnostics.Generator.Core.Annotations.ActivityNoEventAttribute";
        }
        public static class EventSourceAccesstorInstanceAttribute
        {
            public const string FullName = "Diagnostics.Generator.Core.Annotations.EventSourceAccesstorInstanceAttribute";
        }
        public static class LoggerMessageAttribute
        {
            public const string FullName = "Microsoft.Extensions.Logging.LoggerMessageAttribute";
        }
        public static class MapToEventSourceAttribute
        {
            public const string FullName = "Diagnostics.Generator.Core.Annotations.MapToEventSourceAttribute";

            public const string GenerateIds = "GenerateIds";
            public const string GenerateIdStart = "GenerateIdStart";
            public const string GenerateIdClassAccessibility = "GenerateIdClassAccessibility";
        }
        public static class MeterRecordAttribute
        {
            public const string FullName = "Diagnostics.Generator.Core.Annotations.MeterRecordAttribute";
        }
        public static class MeterGenerateAttribute
        {
            public const string FullName = "Diagnostics.Generator.Core.Annotations.MeterGenerateAttribute";
        }
        public static class ActivitySpecialValueAttribute
        {
            public const string FullName = "Diagnostics.Generator.Core.Annotations.ActivitySpecialValueAttribute";

            public const string FilePath = "FilePath";
            public const string MemberName = "MemberName";
            public const string LineNumber = "LineNumber";
        }
        public static class ActivityAsAttribute
        {
            public const string FullName = "Diagnostics.Generator.Core.Annotations.ActivityAsAttribute";

            public const string As = "As";
            public const string Key = "Key";
            public const string TargetType = "TargetType";
            public const string IgnorePaths = "IgnorePaths";
            public const string GenerateSingleton = "GenerateSingleton";
        }
        public static class ActivityAsNameAttribute
        {
            public const string FullName = "Diagnostics.Generator.Core.Annotations.ActivityAsNameAttribute";
        }
        public static class ActivityAsIgnoreAttribute
        {
            public const string FullName = "Diagnostics.Generator.Core.Annotations.ActivityAsIgnoreAttribute";
        }
        public static class MapToEventSourceGenerateIdIgnoreAttribute
        {
            public const string FullName = "Diagnostics.Generator.Core.Annotations.MapToEventSourceGenerateIdIgnoreAttribute";
        }
        public static class MapToEventSourceGenerateIdSpecialAttribute
        {
            public const string FullName = "Diagnostics.Generator.Core.Annotations.MapToEventSourceGenerateIdSpecialAttribute";
        }
        public static class ActivityStatusAttribute
        {
            public const string FullName = "Diagnostics.Generator.Core.Annotations.ActivityStatusAttribute";

            public const string WithDescript = "WithDescript";
        }
    }
}
