using FastBIRe.Cdc.NpgSql;
using Npgsql.Replication;
using Npgsql.Replication.PgOutput;
using rsa;

namespace FastBIRe.CdcSample
{
    public class PgSqlTester
    {
        public async Task Start()
        {
            var mysql = ConnectionProvider.GetDbMigration(DatabaseSchemaReader.DataSchema.SqlType.PostgreSql, "test1");
            var rconn = new LogicalReplicationConnection($"host=192.168.1.101;port=5432;username=postgres;password=Syc123456.;Database=test1");
            await rconn.Open();

            //var slot = new PgOutputReplicationSlot("juhe_slot");

            ////The following will loop until the cancellation token is triggered, and will print message types coming from PostgreSQL:
            //var cancellationTokenSource = new CancellationTokenSource();
            //await foreach (var message in rconn.StartReplication(
            //    slot, new PgOutputReplicationOptions("juhe_pub", 1), cancellationTokenSource.Token))
            //{
            //    Console.WriteLine($"Received message type: {message.GetType().Name}");

            //    // Always call SetReplicationStatus() or assign LastAppliedLsn and LastFlushedLsn individually
            //    // so that Npgsql can inform the server which WAL files can be removed/recycled.
            //    rconn.SetReplicationStatus(message.WalEnd);
            //}

            var mgr = new PgSqlCdcManager(new DefaultScriptExecuter(mysql));
            Console.WriteLine(await mgr.IsDatabaseSupportAsync());
            Console.WriteLine(await mgr.IsDatabaseCdcEnableAsync("test1"));
            Console.WriteLine(await mgr.IsTableCdcEnableAsync("test1", "juhe"));
            await mgr.TryDisableTableCdcAsync("test1", "juhe_effect");
            await mgr.TryEnableTableCdcAsync("test1", "juhe_effect");
            var slotName = "juhe" + PgSqlCdcManager.SlotTail;
            var pubName = "juhe" + PgSqlCdcManager.PubTail;
            var listener = await mgr.GetCdcListenerAsync(new PgSqlGetCdcListenerOptions(
                rconn,
                new PgOutputReplicationSlot(slotName!),
                new PgOutputReplicationOptions(pubName!, 1),
                null,
                null,
                null));
            listener.EventRaised += Program.Vars_EventRaised;
            await listener.StartAsync();
            Console.ReadLine();
        }
    }
}
