using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger _logger;

        public MailClient(IMailClientConfiguration configuration, ILogger logger = null)
        {
            _configuration = configuration;

            _imap = new ImapClient();
            _smtp = new SmtpClient();

            _logger = logger;

            _imap.ServerCertificateValidationCallback += UnconditionalCertificateAcceptance;
            _smtp.ServerCertificateValidationCallback += UnconditionalCertificateAcceptance;
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
                _logger?.LogDebug($"Connecting {nameof(ImapClient)}.");
                await imap.ConnectAsync(configuration.Host, configuration.ImapPort, SocketOptions);
            }

            if (!imap.IsAuthenticated)
            {
                _logger?.LogDebug($"Authenticating {nameof(ImapClient)}.");
                await imap.AuthenticateAsync(configuration.UserName, configuration.Password);
            }

            if (!imap.Inbox.IsOpen)
            {
                _logger?.LogDebug($"Opening Inbox of {nameof(ImapClient)}.");
                await imap.Inbox.OpenAsync(FolderAccess.ReadWrite);
            }
        }

        private async Task EnsureConnectionOf(SmtpClient smtp, IMailClientConfiguration configuration)
        {
            if (!smtp.IsConnected)
            {
                _logger?.LogDebug($"Connecting {nameof(SmtpClient)}.");
                await smtp.ConnectAsync(configuration.Host, configuration.SmtpPort, SocketOptions);
            }

            if (!smtp.IsAuthenticated)
            {
                _logger?.LogDebug($"Authenticating {nameof(SmtpClient)}.");
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
                _logger?.LogDebug($"Disconnecting {nameof(SmtpClient)}.");
                await _smtp.DisconnectAsync(true);
            }
        }
    }
}
