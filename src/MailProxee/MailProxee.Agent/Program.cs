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
                imap.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;

                await imap.ConnectAsync(configuration.Host, configuration.ImapPort, SocketOptions);
                await imap.AuthenticateAsync(configuration.UserName, configuration.Password);

                smtp.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;

                await smtp.ConnectAsync(configuration.Host, configuration.SmtpPort, SocketOptions);
                await smtp.AuthenticateAsync(configuration.UserName, configuration.Password);

                await imap.Inbox.OpenAsync(FolderAccess.ReadWrite);

                var messageHandler = Task.Run(async () =>
                {
                    var categorizer = new MessageCategorizer(configuration);

                    while (!_tokenSource.Token.IsCancellationRequested)
                    {
                        var messages = await imap.Inbox.FetchAsync(0, imap.Inbox.Count, MessageSummaryItems.All);

                        foreach (var message in messages)
                        {
                            var category = categorizer.Categorize(message.Envelope);

                            switch (category)
                            {
                                case MessageCategory.Request:
                                    await HandleRequestMessage(message.Envelope, smtp, configuration)
                                        .ConfigureAwait(false);
                                    break;
                                case MessageCategory.Incoming:
                                    await HandleIncoming(message, smtp, configuration)
                                        .ConfigureAwait(false);
                                    break;
                            }
                            
                            await imap.Inbox.AddFlagsAsync(message.Index, MessageFlags.Deleted, true);
                        }

                        await imap.Inbox.ExpungeAsync()
                            .ConfigureAwait(false);

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

                await messageHandler;

                await imap.DisconnectAsync(true);
                await smtp.DisconnectAsync(true);
            }
        }

        private static async Task HandleIncoming(IMessageSummary message, SmtpClient smtp, Configuration configuration)
        {
            var envelope = message.Envelope;

            var destinations = envelope.To.Mailboxes
                .Select(mailboxAddress => mailboxAddress.Address)
                .Select(address => address.Split('@').FirstOrDefault())
                .Where(identifier => identifier is object)
                .Where(identifier => Guid.TryParse(identifier, out var _))
                .Select(identifier => Guid.Parse(identifier))
                .Where(guid => _forwarding.ContainsKey(guid))
                .Select(guid => _forwarding[guid]);

            foreach (var destination in destinations)
            {
                var alias = Guid.NewGuid().ToString();

                var from = new[] { new MailboxAddress($"{alias}@{configuration.ReplyDomain}") };
                var to = new[] { new MailboxAddress(destination) };

                var forwardMessage = new MimeMessage(from, to, envelope.Subject, message.Body);

                await smtp.SendAsync(forwardMessage);
            }
        }

        private static async Task HandleRequestMessage(Envelope envelope, SmtpClient smtp, Configuration configuration)
        {
            var request = new AliasRequest(envelope, configuration);

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
