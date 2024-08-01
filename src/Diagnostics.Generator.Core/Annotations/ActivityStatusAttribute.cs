using System;
using System.Diagnostics;

namespace Diagnostics.Generator.Core.Annotations
{
    [AttributeUsage(AttributeTargets.Method,AllowMultiple =false,Inherited = false)]
    public sealed class ActivityStatusAttribute : Attribute
    {
        public ActivityStatusAttribute(ActivityStatusCode status)
        {
            Status = status;
        }

        public ActivityStatusCode Status { get; }

        public bool WithDescript { get; set; }
    }
}
