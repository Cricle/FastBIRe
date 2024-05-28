using System;
#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Diagnostics.Generator.Core.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class ActivityTagAttribute : Attribute
    {
        public ActivityTagAttribute(string name,
#if NET8_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)] 
#endif
        string expression)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public string Name { get; }

        public string Expression { get; }

        public bool IsSet { get; set; }

        public bool IsAdd { get; set; }
    }
}
