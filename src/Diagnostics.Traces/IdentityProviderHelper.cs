using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using System.Diagnostics;

namespace Diagnostics.Traces
{
    public static class IdentityProviderHelper
    {
        public static IIdentityProvider<string, Activity> ActivityByTagObjects(string key, string? sourceName = null)
        {
            return ActivityByTagObjects(x => x == key, x => x.ToString(), sourceName);
        }
        public static IIdentityProvider<string, LogRecord> LogByAttribute(string key)
        {
            return LogByAttribute(x => x == key, x => x.ToString());
        }
        public static IIdentityProvider<string, Metric> MetricByTags(string key, string? meterName = null)
        {
            return MetricByTags(x => x == key, x => x.ToString(), meterName);
        }
        public static IIdentityProvider<TIdentity, Activity> ActivityByTagObjects<TIdentity>(Func<string, bool> equals, Func<object, TIdentity?> caster, string? sourceName = null)
            where TIdentity : IEquatable<TIdentity>
        {
            var hasSourceName = !string.IsNullOrEmpty(sourceName);
            return new DelegateIdentityProvider<TIdentity, Activity>(s =>
            {
                if (hasSourceName && s.Source.Name != sourceName)
                {
                    return GetIdentityResult<TIdentity>.Fail;
                }
                foreach (var item in s.TagObjects)
                {
                    if (item.Value != null && equals(item.Key))
                    {
                        return new GetIdentityResult<TIdentity>(caster(item.Value));
                    }
                }
                return GetIdentityResult<TIdentity>.Fail;
            });
        }
        public static IIdentityProvider<TIdentity, LogRecord> LogByAttribute<TIdentity>(Func<string, bool> equals, Func<object, TIdentity?> caster)
            where TIdentity : IEquatable<TIdentity>
        {
            return new DelegateIdentityProvider<TIdentity, LogRecord>(s =>
            {
                if (s.Attributes != null)
                {
                    foreach (var item in s.Attributes)
                    {
                        if (item.Value != null && equals(item.Key))
                        {
                            return new GetIdentityResult<TIdentity>(caster(item.Value));
                        }
                    }
                }
                return GetIdentityResult<TIdentity>.Fail;
            });
        }
        public static IIdentityProvider<TIdentity, Metric> MetricByTags<TIdentity>(Func<string, bool> equals, Func<object, TIdentity?> caster, string? meterName = null)
            where TIdentity : IEquatable<TIdentity>
        {
            var hasSourceName = !string.IsNullOrEmpty(meterName);
            return new DelegateIdentityProvider<TIdentity, Metric>(s =>
            {
                if (hasSourceName && s.MeterName != meterName)
                {
                    return GetIdentityResult<TIdentity>.Fail;
                }
                foreach (ref readonly var point in s.GetMetricPoints())
                {
                    foreach (var tag in point.Tags)
                    {
                        if (equals(tag.Key) && tag.Value != null)
                        {
                            return new GetIdentityResult<TIdentity>(caster(tag.Value));
                        }
                    }
                }
                return GetIdentityResult<TIdentity>.Fail;
            });
        }
    }
}
