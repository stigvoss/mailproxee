using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
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

        public async Task PrepareConnection()
        {
            await _imap.ConnectAsync(_configuration.Host, _configuration.ImapPort, SocketOptions);
            await _imap.AuthenticateAsync(_configuration.UserName, _configuration.Password);

            await _smtp.ConnectAsync(_configuration.Host, _configuration.SmtpPort, SocketOptions);
            await _smtp.AuthenticateAsync(_configuration.UserName, _configuration.Password);

            await _imap.Inbox.OpenAsync(FolderAccess.ReadWrite);
        }

        internal async Task Send(Message message)
        {
            var mime = (MimeMessage)message;
            
            await _smtp.SendAsync(mime)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<Message>> FetchMessages()
        {
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
            await _imap.Inbox.ExpungeAsync();
        }

        internal async Task PermitDeletion(Message message)
        {
            if (message.UniqueId.HasValue)
            {
                await _imap.Inbox.AddFlagsAsync(message.UniqueId.Value, MessageFlags.Deleted, true);
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
