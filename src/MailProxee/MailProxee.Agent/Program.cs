using CommandLine;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MailProxee.Agent
{
    class Program
    {
        private const SecureSocketOptions SocketOptions = SecureSocketOptions.Auto;

        private static readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        private static readonly ConcurrentDictionary<Guid, string> _forwarding = new ConcurrentDictionary<Guid, string>();

        static async Task Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<Options>(args);

            if (result is Parsed<Options> options)
            {
                var configuration = await Configuration.Load(options.Value.Configuration);
                await Run(configuration);
            }
        }

        private static async Task Run(Configuration configuration)
        {
            using (var imap = new ImapClient())
            using (var smtp = new SmtpClient())
            {
                imap.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await imap.ConnectAsync(configuration.Host, configuration.ImapPort, SocketOptions);
                await imap.AuthenticateAsync(configuration.UserName, configuration.Password);

                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await smtp.ConnectAsync(configuration.Host, configuration.SmtpPort, SocketOptions);
                await smtp.AuthenticateAsync(configuration.UserName, configuration.Password);

                imap.Inbox.Open(FolderAccess.ReadWrite);

                var task = Task.Run(async () =>
                {
                    var categorizer = new MessageCategorizer(configuration);

                    while (!_tokenSource.Token.IsCancellationRequested)
                    {
                        for (var index = 0; index < imap.Inbox.Count; index++)
                        {
                            var message = await imap.Inbox.GetMessageAsync(index);

                            var category = categorizer.Categorize(message);

                            switch (category)
                            {
                                case MessageCategory.Request:
                                    await HandleRequestMessage(message, smtp, configuration);
                                    break;
                                case MessageCategory.Incoming:
                                    await HandleIncoming(message, smtp, configuration);
                                    break;
                            }

                            await imap.Inbox.AddFlagsAsync(index, MessageFlags.Deleted, true);
                        }
                        await imap.Inbox.ExpungeAsync();

                        try
                        {
                            await Task.Delay(10000, _tokenSource.Token);
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                        }
                    }
                });

                Console.WriteLine("Press [Enter] to exit...");
                Console.ReadLine();

                _tokenSource.Cancel();

                await task;

                await imap.DisconnectAsync(true);
                await smtp.DisconnectAsync(true);
            }
        }

        private static async Task HandleIncoming(MimeMessage message, SmtpClient smtp, Configuration configuration)
        {
            var destinations = message.To.Mailboxes
                .Select(mailboxAddress => mailboxAddress.Address)
                .Select(address => address.Split('@').FirstOrDefault())
                .Where(identifier => identifier is object)
                .Where(identifier => Guid.TryParse(identifier, out var _))
                .Select(identifier => Guid.Parse(identifier))
                .Where(guid => _forwarding.ContainsKey(guid))
                .Select(guid => _forwarding[guid]);

            foreach (var destination in destinations)
            {
                var from = new[] { new MailboxAddress($"{Guid.NewGuid().ToString()}@{configuration.ReplyDomain}") };
                var to = new[] { new MailboxAddress(destination) };

                var forwardMessage = new MimeMessage(from, to, message.Subject, message.Body);

                await smtp.SendAsync(forwardMessage);
            }
        }

        private static async Task HandleRequestMessage(MimeMessage message, SmtpClient smtp, Configuration configuration)
        {
            var request = new AliasRequest(message, configuration);

            if (request.IsExpectedRecipient())
            {
                var alias = Guid.NewGuid();
                var response = request.NewResponseWith(alias);

                _forwarding.TryAdd(alias, response.Recipient.Address);

                await smtp.SendAsync(response);
            }
        }
    }

    public class Options
    {
        [Option('c', "config", Required = true)]
        public string Configuration { get; set; }
    }

    public class Configuration
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

        public static async Task<Configuration> Load(string path)
        {
            var content = await File.ReadAllTextAsync(path);
            return JsonConvert.DeserializeObject<Configuration>(content);
        }
    }
}
