using Microsoft.Diagnostics.NETCore.Client;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.Helpers
{
    public static class TraceHelper
    {
        public static async Task TraceAsync(int processId, IEnumerable<EventPipeProvider> providers, string filePath, bool requestRundown = true, int circularBufferMB = 256, CancellationToken token = default)
        {
            using (var fs = File.Open(filePath, FileMode.Create))
            {
                await TraceAsync(processId, providers, fs, requestRundown, circularBufferMB, token);
            }
        }
        public static async Task TraceAsync(int processId, IEnumerable<EventPipeProvider> providers, Stream outStream, bool requestRundown = true, int circularBufferMB = 256, CancellationToken token = default)
        {
            var client = new DiagnosticsClient(processId);
            using (var session = client.StartEventPipeSession(providers, requestRundown, circularBufferMB))
            {
                if (requestRundown)
                {
                    client.ResumeRuntime();
                }
                token.Register(() => session.Stop());
                await session.EventStream.CopyToAsync(outStream,81920,token);
            }
        }
    }
}
