using MimeKit;
using System;
using System.Linq;

namespace MailProxee.RequestAgent
{
    internal class AliasRequest
    {
        private readonly MimeMessage _message;
        private readonly string _domain;
        private readonly string _incomingDomain;
        private readonly string _requestAddress;

        public AliasRequest(MimeMessage mime, Configuration configuration)
        {
            _message = mime;
            _domain = configuration.Domain;
            _incomingDomain = configuration.IncomingDomain;
            _requestAddress = configuration.RequestAddress;
        }

        public bool IsExpectedRecipient()
        {
            return _message.To.Count == 1
                && _message.To.Mailboxes.Any(e => e.Address == _requestAddress);
        }

        public AliasResponse NewResponseWith(Guid guid)
        {
            var message = new AliasResponse
            {
                Subject = $"A {_domain} alias has been created as requested.",
                Recipient = _message.From.FirstOrDefault() as MailboxAddress,
                Sender = _message.To.FirstOrDefault() as MailboxAddress,
                Content = $"{guid.ToString()}@{_incomingDomain}"
            };

            return message;
        }
    }
}