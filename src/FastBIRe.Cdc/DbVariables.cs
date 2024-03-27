using System;
using System.Collections.Generic;

namespace FastBIRe.Cdc
{
    public class DbVariables : Dictionary<string, string?>
    {
        public DbVariables()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }
        public virtual T? GetAndCase<T>(string key)
        {
            var val = GetOrDefault(key);
            if (val == null)
            {
                return default;
            }
            return (T)Convert.ChangeType(val, typeof(T));
        }
        public virtual bool GetAndEquals(string key, string value)
        {
            var val = GetOrDefault(key);
            if (val == null)
            {
                return false;
            }
            return string.Equals(val, value, StringComparison.OrdinalIgnoreCase);
        }

        public virtual string? GetOrDefault(string key)
        {
            if (TryGetValue(key, out var val))
            {
                return val;
            }
            return null;
        }
    }
}
