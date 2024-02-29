using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing.Parsers;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace Diagnostics.Helpers
{
    public static class WellKnowsEventProvider
    {
        public const string DotNetRuntime = "Microsoft-Windows-DotNETRuntime";
        public const string AspNetCoreHosting = "Microsoft.AspNetCore.Hosting";
        public const string Kestrel = "Microsoft-AspNetCore-Server-Kestrel";
        public const string DotNetRuntimePrivate = "Microsoft-Windows-DotNETRuntimePrivate";

        public const string Runtime = "System.Runtime";
        public const string Database = "database";

        public const string Sample = "Microsoft-DotNETCore-SampleProfiler";

        public static IReadOnlyList<EventPipeProvider> CpuSamplingProviders => new[]
        {
            new EventPipeProvider(Sample, EventLevel.Informational),
            new EventPipeProvider(DotNetRuntime, EventLevel.Informational, (long)ClrTraceEventParser.Keywords.Default),
        };
        public static IReadOnlyList<EventPipeProvider> GCVerboseProviders { get; }= new[]
        {
            new EventPipeProvider(DotNetRuntime, EventLevel.Verbose,
                keywords: (long)ClrTraceEventParser.Keywords.GC |
                          (long)ClrTraceEventParser.Keywords.GCHandle |
                          (long)ClrTraceEventParser.Keywords.Exception)
        };
        public static IReadOnlyList<EventPipeProvider> GCCollectProviders { get; } = new[]
        {
            new EventPipeProvider(
                name: DotNetRuntime,
                eventLevel: EventLevel.Informational,
                keywords: (long)ClrTraceEventParser.Keywords.GC
            ),
            new EventPipeProvider(
                name: DotNetRuntimePrivate,
                eventLevel: EventLevel.Informational,
                keywords: (long)ClrTraceEventParser.Keywords.GC
            )
        };
        public static IReadOnlyList<EventPipeProvider> DatabaseProviders { get; } = new[]
        {
            new EventPipeProvider(
                name: "System.Threading.Tasks.TplEventSource",
                eventLevel: EventLevel.Informational,
                keywords: (long)TplEtwProviderTraceEventParser.Keywords.TasksFlowActivityIds
            ),
            new EventPipeProvider(
                name: "Microsoft-Diagnostics-DiagnosticSource",
                eventLevel: EventLevel.Verbose,
                keywords:   (long)DiagnosticSourceKeywords.Messages |
                            (long)DiagnosticSourceKeywords.Events,
                arguments: new Dictionary<string, string> {
                    {
                        "FilterAndPayloadSpecs",
                            "SqlClientDiagnosticListener/System.Data.SqlClient.WriteCommandBefore@Activity1Start:-Command;Command.CommandText;ConnectionId;Operation;Command.Connection.ServerVersion;Command.CommandTimeout;Command.CommandType;Command.Connection.ConnectionString;Command.Connection.Database;Command.Connection.DataSource;Command.Connection.PacketSize\r\n" +
                            "SqlClientDiagnosticListener/System.Data.SqlClient.WriteCommandAfter@Activity1Stop:\r\n" +
                            "Microsoft.EntityFrameworkCore/Microsoft.EntityFrameworkCore.Database.Command.CommandExecuting@Activity2Start:-Command.CommandText;Command;ConnectionId;IsAsync;Command.Connection.ClientConnectionId;Command.Connection.ServerVersion;Command.CommandTimeout;Command.CommandType;Command.Connection.ConnectionString;Command.Connection.Database;Command.Connection.DataSource;Command.Connection.PacketSize\r\n" +
                            "Microsoft.EntityFrameworkCore/Microsoft.EntityFrameworkCore.Database.Command.CommandExecuted@Activity2Stop:"
                    }
                }
            )
        };
        /// <summary>
        /// Keywords for DiagnosticSourceEventSource provider
        /// </summary>
        /// <remarks>See https://github.com/dotnet/corefx/blob/master/src/System.Diagnostics.DiagnosticSource/src/System/Diagnostics/DiagnosticSourceEventSource.cs</remarks>
        private enum DiagnosticSourceKeywords : long
        {
            Messages = 0x1,
            Events = 0x2,
            IgnoreShortCutKeywords = 0x0800,
            AspNetCoreHosting = 0x1000,
            EntityFrameworkCoreCommands = 0x2000
        }
    }
}
