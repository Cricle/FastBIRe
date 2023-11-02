using rsa;
using FastBIRe.Cdc.Triggers;
using FastBIRe.Cdc.Mssql;

namespace FastBIRe.CdcSample
{
    public class TriggerTester
    {
        public async Task Start()
        {
            var mysql = ConnectionProvider.GetDbMigration(DatabaseSchemaReader.DataSchema.SqlType.MySql, "ttt");
            var executer = new DefaultScriptExecuter(mysql);
            var tcdc=new TriggerCdcManager(executer);
            await tcdc.TryEnableTableCdcAsync("ttt", "guidang");
            var listener = await tcdc.GetCdcListenerAsync(new TriggerGetCdcListenerOptions(executer,new string[]
            {
                "guidang_affect"
            },TimeSpan.FromSeconds(1),10));
            listener.EventRaised += Program.Vars_EventRaised;
            await listener.StartAsync();
            Console.ReadLine();
        }
    }
}
