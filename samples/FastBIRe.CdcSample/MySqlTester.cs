using FastBIRe.Cdc.MySql;
using MySqlConnector;

namespace FastBIRe.CdcSample
{
    public class MySqlTester
    {
        public async Task Start()
        {
            var mysql = new MySqlConnection("Server=192.168.1.101;Port=3306;Uid=root;Pwd=Syc123456.;Connection Timeout=2000;Character Set=utf8;Database=test2");
            mysql.Open();

            var mysqlCfg = new MySqlConnectionStringBuilder(mysql.ConnectionString);

            var mgr = new MySqlCdcManager(new DefaultScriptExecuter(mysql), MySqlCdcModes.Gtid);
            //var ser =await mgr.GetCdcLogServiceAsync();
            //var all=await ser.GetAllAsync();
            //var last=await ser.GetLastAsync();
            var vars = await mgr.GetCdcListenerAsync(new MySqlGetCdcListenerOptions(null, null, opt =>
            {
                opt.Port = (int)mysqlCfg.Port;
                opt.Hostname = mysqlCfg.Server;
                opt.Password = "Syc123456.";
                opt.Username = mysqlCfg.UserID;
                opt.Database = mysqlCfg.Database;
                opt.ServerId = 1;
            }));
            vars.EventRaised += Program.Vars_EventRaised;
            await vars.StartAsync();
            Console.ReadLine();
        }
    }
}
