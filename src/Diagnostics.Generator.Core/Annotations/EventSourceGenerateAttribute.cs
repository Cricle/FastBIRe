﻿using System;

namespace Diagnostics.Generator.Core.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class EventSourceGenerateAttribute : Attribute
    {
        public bool IncludeInterface { get; set; }

        public Accessibility InterfaceAccessibility { get; set; }
    }
}
