using Module.EmailProxy.Application;
using Module.EmailProxy.Infrastructure.Base;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace MailProxee.Agent
{
    public class Configuration : IMailboxHandlerConfiguration
    {
        public string Domain { get; set; }

        public string IncomingPrefix { get; set; }

        public string ReplyPrefix { get; set; }

        public string RequestMailbox { get; set; }

        public string IncomingDomain
            => $"{IncomingPrefix}.{Domain}";

        public string ReplyDomain
            => $"{ReplyPrefix}.{Domain}";

        public string RequestAddress
            => $"{RequestMailbox}@{Domain}";

        public string Host { get; set; }

        public int ImapPort { get; set; }

        public int SmtpPort { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public DatabaseConfiguration Database { get; set; }

        public static async Task<Configuration> LoadFrom(string path)
        {
            var content = await File.ReadAllTextAsync(path);
            return JsonConvert.DeserializeObject<Configuration>(content);
        }
    }

    public class DatabaseConfiguration : IDataSourceConfiguration
    {
        public string Host { get; set; }

        public ushort Port { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string DataSource { get; set; }
    }
}
