using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Module.EmailProxy.Domain;
using Module.EmailProxy.Infrastructure.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Module.EmailProxy.Infrastructure
{
    public class MailClient : IDisposable
    {
        private const SecureSocketOptions SocketOptions = SecureSocketOptions.Auto;

        private readonly IMailClientConfiguration _configuration;
        private readonly ImapClient _imap;
        private readonly SmtpClient _smtp;

        public MailClient(IMailClientConfiguration configuration)
        {
            _configuration = configuration;

            _imap = new ImapClient();
            _smtp = new SmtpClient();

            _imap.ServerCertificateValidationCallback += UnconditionalCertificateAcceptance;
            _smtp.ServerCertificateValidationCallback += UnconditionalCertificateAcceptance;
        }

        public async Task PrepareMailboxConnection()
        {
            await EnsureConnectionOf(_imap, _configuration);
            await EnsureConnectionOf(_smtp, _configuration);
        }

        internal async Task Send(Message message)
        {
            await EnsureConnectionOf(_smtp, _configuration);

            var mime = (MimeMessage)message;

            await _smtp.SendAsync(mime)
                .ConfigureAwait(false);
        }

        public async Task<ICollection<Message>> FetchMessages()
        {
            await EnsureConnectionOf(_imap, _configuration);

            var summaries = await _imap.Inbox
                .FetchAsync(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.Full);

            var messages = new Message[summaries.Count];

            for (var index = 0; index < summaries.Count; index++)
            {
                var summary = summaries[index];

                var message = await _imap.Inbox.GetMessageAsync(summary.UniqueId);
                messages[index] = new Message(message, summary);
            }

            return messages;
        }

        internal async Task DeleteMessages()
        {
            await EnsureConnectionOf(_imap, _configuration);

            await _imap.Inbox.ExpungeAsync();
        }

        internal async Task PermitDeletion(Message message)
        {
            await EnsureConnectionOf(_imap, _configuration);

            if (message.UniqueId.HasValue)
            {
                await _imap.Inbox.AddFlagsAsync(message.UniqueId.Value, MessageFlags.Deleted, true);
            }
        }

        private async Task EnsureConnectionOf(ImapClient imap, IMailClientConfiguration configuration)
        {
            if (!imap.IsConnected)
            {
                await imap.ConnectAsync(configuration.Host, configuration.ImapPort, SocketOptions);
            }

            if (!imap.IsAuthenticated)
            {
                await imap.AuthenticateAsync(configuration.UserName, configuration.Password);
            }

            if (!imap.Inbox.IsOpen)
            {
                await imap.Inbox.OpenAsync(FolderAccess.ReadWrite);
            }
        }

        private async Task EnsureConnectionOf(SmtpClient smtp, IMailClientConfiguration configuration)
        {
            if (!smtp.IsConnected)
            {
                await smtp.ConnectAsync(configuration.Host, configuration.SmtpPort, SocketOptions);
            }

            if (!smtp.IsAuthenticated)
            {
                await smtp.AuthenticateAsync(configuration.UserName, configuration.Password);
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

        internal async Task DisconnectSmtp()
        {
            if (_smtp.IsConnected)
            {
                await _smtp.DisconnectAsync(true);
            }
        }
    }
}
