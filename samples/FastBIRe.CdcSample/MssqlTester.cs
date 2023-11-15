using FastBIRe.Cdc.Mssql;
using rsa;

namespace FastBIRe.CdcSample
{
    public class MssqlTester
    {
        public async Task Start()
        {
            var mssql = ConnectionProvider.GetDbMigration(DatabaseSchemaReader.DataSchema.SqlType.SqlServer, "test11");
            var comm = new MssqlCdcManager(() => new DefaultScriptExecuter(mssql));
            await comm.TryEnableDatabaseCdcAsync("test11");
            await comm.TryEnableTableCdcAsync("test11", "juhe");
            var listen = await comm.GetCdcListenerAsync(new MssqlGetCdcListenerOptions(TimeSpan.FromSeconds(1), comm.ScriptExecuterFactory(), null, null));
            listen.EventRaised += Program.Vars_EventRaised;
            await listen.StartAsync();
            Console.ReadLine();
        }
    }
}
