using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastBIRe.Cdc.Mssql
{
    public class MssqlVariables : DbVariables
    {
        public const string AgentStateKey = "AgentState";

        public MssqlServiceState? AgentState
        {
            get
            {
                var state = GetOrDefault(AgentStateKey);
                if (string.IsNullOrEmpty(state))
                {
                    return null;
                }
                if (state.StartsWith("running", StringComparison.OrdinalIgnoreCase))
                {
                    return MssqlServiceState.Running;
                }
                return MssqlServiceState.Stopped;
            }
        }
    }
    public enum MssqlServiceState
    {
        Running,
        Stopped
    }
}
