using System;

namespace Diagnostics.Generator.Core.Annotations
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class CounterAttribute : Attribute
    {
        public CounterAttribute(string name, CounterTypes type)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(nameof(name));
            }

            Name = name;
            Type = type;
        }

        public string Name { get; }

        public CounterTypes Type { get; }

        public double DisplayRateTimeScaleMs { get; set; }

        public string DisplayName { get; set; }

        public string DisplayUnits { get; set; }
    }
}
