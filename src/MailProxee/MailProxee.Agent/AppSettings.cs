using Module.EmailProxy.Application;
using Module.EmailProxy.Infrastructure.Base;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace MailProxee.Agent
{
    public class AppSettings
    {
        public MailboxConfiguration Mailbox { get; set; } = new MailboxConfiguration();

        public DatabaseConfiguration Database { get; set; } = new DatabaseConfiguration();

        public static async Task<AppSettings> LoadFrom(string path)
        {
            var content = await File.ReadAllTextAsync(path);
            return JsonConvert.DeserializeObject<AppSettings>(content);
        }
    }

    public class MailboxConfiguration : IMailboxHandlerConfiguration
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
