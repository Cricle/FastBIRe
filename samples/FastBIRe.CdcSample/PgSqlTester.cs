using FastBIRe.Cdc.NpgSql;
using Npgsql.Replication.PgOutput;
using Npgsql.Replication;
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
            var mgr = new PgSqlCdcManager(new DefaultScriptExecuter(mysql));
            var listener = await mgr.GetCdcListenerAsync(new PgSqlGetCdcListenerOptions(
                rconn,
                new PgOutputReplicationSlot("blog_slot"),
                new PgOutputReplicationOptions("blog_pub", 1),
                null,
                null));
            listener.EventRaised += Program.Vars_EventRaised;
            await listener.StartAsync();
            Console.ReadLine();
        }
    }
}
