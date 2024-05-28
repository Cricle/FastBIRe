using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace Diagnostics.Helpers
{
    public static class EventPipeProviderBuilderSetExtensions
    {
        public static IEventPipeProviderBuilder WhenName(this IEventPipeProviderBuilder builder, string name, Func<IEventPipeProviderBuilder, IEventPipeProviderBuilder> handle,StringComparison comparison= StringComparison.Ordinal)
        {
            if (builder.Name.Equals(name, comparison))
            {
                return handle(builder);
            }
            return builder;
        }
        public static IEventPipeProviderBuilder WithKeywords(this IEventPipeProviderBuilder builder, long keywords)
        {
            builder.Keywords = keywords;
            return builder;
        }
        public static IEventPipeProviderBuilder WithEventLevel(this IEventPipeProviderBuilder builder, EventLevel level)
        {
            builder.EventLevel = level;
            return builder;
        }
        public static IEventPipeProviderBuilder SetArguments(this IEventPipeProviderBuilder builder, IDictionary<string, string>? arguments)
        {
            builder.Arguments = arguments;
            return builder;
        }
        public static IEventPipeProviderBuilder WithArguments(this IEventPipeProviderBuilder builder, IDictionary<string, string> arguments)
        {
            if (builder.Arguments==null)
            {
                builder.Arguments = arguments;
            }
            else
            {
                foreach (var item in arguments)
                {
                    builder.Arguments[item.Key] = item.Value;
                }
            }
            return builder;
        }
        public static IEventPipeProviderBuilder WithArguments(this IEventPipeProviderBuilder builder, string name,string value)
        {
            if (builder.Arguments == null)
            {
                builder.Arguments = new Dictionary<string, string>
                {
                    [name] = value
                };
            }
            else
            {
                builder.Arguments[name] = value;
            }
            return builder;
        }
    }
}
