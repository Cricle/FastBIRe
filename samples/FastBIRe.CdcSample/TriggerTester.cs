using FastBIRe.Cdc.Mssql;
using FastBIRe.Cdc.Triggers;
using rsa;

namespace FastBIRe.CdcSample
{
    public class TriggerTester
    {
        public async Task Start()
        {
            var mysql = ConnectionProvider.GetDbMigration(DatabaseSchemaReader.DataSchema.SqlType.MySql, "ttt");
            var executer = new DefaultScriptExecuter(mysql);
            var tcdc = new TriggerCdcManager(executer);
            await tcdc.TryEnableTableCdcAsync("ttt", "guidang");
            var listener = await tcdc.GetCdcListenerAsync(new TriggerGetCdcListenerOptions(executer, TimeSpan.FromSeconds(1), 10, null, new string[]
            {
                "guidang_affect"
            }));
            listener.EventRaised += Program.Vars_EventRaised;
            await listener.StartAsync();
            Console.ReadLine();
        }
    }
}
