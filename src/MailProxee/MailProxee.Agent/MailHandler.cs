using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace MailProxee.Agent
{
    internal class MailHandler : IDisposable
    {
        private const SecureSocketOptions SocketOptions = SecureSocketOptions.Auto;

        private static readonly ConcurrentDictionary<Guid, string> _forwarding = new ConcurrentDictionary<Guid, string>();
        
        private readonly Configuration _configuration;
        private readonly ImapClient _imap;
        private readonly SmtpClient _smtp;

        public MailHandler(Configuration configuration)
        {
            _configuration = configuration;

            _imap = new ImapClient();
            _smtp = new SmtpClient();

            _imap.ServerCertificateValidationCallback += UnconditionalCertificateAcceptance;
            _smtp.ServerCertificateValidationCallback += UnconditionalCertificateAcceptance;
        }

        public async Task Connect()
        {
            await _imap.ConnectAsync(_configuration.Host, _configuration.ImapPort, SocketOptions);
            await _imap.AuthenticateAsync(_configuration.UserName, _configuration.Password);

            await _smtp.ConnectAsync(_configuration.Host, _configuration.SmtpPort, SocketOptions);
            await _smtp.AuthenticateAsync(_configuration.UserName, _configuration.Password);
        }

        public async Task Open()
        {
            await _imap.Inbox.OpenAsync(FolderAccess.ReadWrite);
        }

        public async Task HandleMailboxMessages(CancellationToken token)
        {
            await Task.Run(async () =>
                {
                    var categorizer = new MessageCategorizer(_configuration);

                    while (!token.IsCancellationRequested)
                    {
                        var messages = await _imap.Inbox.FetchAsync(0, -1, MessageSummaryItems.All);

                        foreach (var message in messages)
                        {
                            var category = categorizer.Categorize(message.Envelope);

                            switch (category)
                            {
                                case MessageCategory.Request:
                                    await HandleRequestMessage(message.Envelope, _smtp, _configuration)
                                        .ConfigureAwait(false);
                                    break;
                                case MessageCategory.Incoming:
                                    await HandleIncoming(message, _smtp, _configuration)
                                        .ConfigureAwait(false);
                                    break;
                            }

                            await _imap.Inbox.AddFlagsAsync(message.Index, MessageFlags.Deleted, true);
                        }

                        await _imap.Inbox.ExpungeAsync()
                            .ConfigureAwait(false);

                        try
                        {
                            await Task.Delay(10000, token);
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                        }
                    }
                }).ConfigureAwait(false);
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

        public void Dispose()
        {
            _imap.Disconnect(true);
            _smtp.Disconnect(true);

            _imap.Dispose();
            _smtp.Dispose();
        }

        private bool UnconditionalCertificateAcceptance(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors errors) => true;
    }
}