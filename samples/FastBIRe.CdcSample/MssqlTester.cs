using FastBIRe.Cdc.Mssql;
using rsa;

namespace FastBIRe.CdcSample
{
    public class MssqlTester
    {
        public async Task Start()
        {
            var mssql = ConnectionProvider.GetDbMigration(DatabaseSchemaReader.DataSchema.SqlType.SqlServer, "test");
            var comm = new MssqlCdcManager(() => new DefaultScriptExecuter(mssql));
            var listen = await comm.GetCdcListenerAsync(new MssqlGetCdcListenerOptions(null, TimeSpan.FromSeconds(1), comm.ScriptExecuterFactory()));
            listen.EventRaised += Program.Vars_EventRaised;
            await listen.StartAsync();
        }
    }
}
