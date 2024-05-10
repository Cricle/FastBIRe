using OpenTelemetry.Logs;
using System.Diagnostics;

namespace Diagnostics.Traces
{
    public static class IdentityProviderHelper
    {
        public static IIdentityProvider<string, Activity> ActivityByTagObjects(string key)
        {
            return ActivityByTagObjects(x => x == key, x => x.ToString());
        }
        public static IIdentityProvider<string, LogRecord> LogByAttribute(string key)
        {
            return LogByAttribute(x => x == key, x => x.ToString());
        }
        public static IIdentityProvider<TIdentity, Activity> ActivityByTagObjects<TIdentity>(Func<string, bool> equals, Func<object, TIdentity> caster)
            where TIdentity : IEquatable<TIdentity>
        {
            return new DelegateIdentityProvider<TIdentity, Activity>(s =>
            {
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
        public static IIdentityProvider<TIdentity, LogRecord> LogByAttribute<TIdentity>(Func<string, bool> equals, Func<object, TIdentity> caster)
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
    }
}
