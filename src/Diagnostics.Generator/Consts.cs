﻿namespace Diagnostics.Generator
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
    }
}