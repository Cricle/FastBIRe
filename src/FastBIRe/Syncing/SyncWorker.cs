using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using FastBIRe.Creating;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace FastBIRe.Syncing
{
    public class SyncWorker
    {
        public SyncWorker(IDbScriptExecuter scriptExecuter)
        {
            ScriptExecuter = scriptExecuter;
            Reader = scriptExecuter.CreateReader();
            SqlType = Reader.SqlType!.Value;
            DatabaseCreateAdapter = SqlType.GetDatabaseCreateAdapter()!;
        }

        public IDbScriptExecuter ScriptExecuter { get; }

        public DatabaseReader Reader { get; }

        public SqlType SqlType { get; }

        public IDatabaseCreateAdapter DatabaseCreateAdapter { get; }

        public virtual async Task<int> SyncStructAsync(DatabaseTable table, CancellationToken token = default)
        {
            var dropTableIfExistsSql = DatabaseCreateAdapter.DropDatabaseIfExists(table.Name);
            var effect = await ScriptExecuter.ExecuteAsync(dropTableIfExistsSql, token: token);

            var ddlScripts = new DdlGeneratorFactory(SqlType).TableGenerator(table).Write();
            effect += await ScriptExecuter.ExecuteAsync(ddlScripts, token: token);
            return effect;
        }

    }
}
