using MySqlCdc;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FastBIRe.Cdc.MySql
{
    public class MySqlCdcListenerOptionCreator : ICdcListenerOptionCreator
    {
        public int Port { get; set; } = 3306;

        public string? Server { get; set; }

        public string? Password { get; set; }

        public string? UserId { get; set; }

        public string? Database { get; set; }

        public Action<ReplicaOptions>? OptionAction { get; set; }

        public Task<ICdcListener> CreateCdcListnerAsync(CdcListenerOptionCreateInfo info, CancellationToken token = default)
        {
            return info.Runner.CdcManager.GetCdcListenerAsync(new MySqlGetCdcListenerOptions(info.CheckPoint, opt =>
            {
                opt.Port = Port;
                opt.Hostname = Server ?? string.Empty;
                opt.Password = Password ?? string.Empty;
                opt.Username = UserId ?? string.Empty;
                opt.Database = Database ?? string.Empty;
                OptionAction?.Invoke(opt);
            }), token);
        }
    }
}
