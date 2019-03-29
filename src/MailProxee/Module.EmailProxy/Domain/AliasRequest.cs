using MailKit;
using MimeKit;
using Module.EmailProxy.Infrastructure;
using System;
using System.Linq;

namespace Module.EmailProxy.Domain
{
    public class AliasRequest
    {
        private readonly Envelope _envelope;
        private readonly string _domain;
        private readonly string _incomingDomain;
        private readonly string _requestAddress;

        public AliasRequest(Envelope envelope, IInternetDomainConfiguration configuration)
        {
            _envelope = envelope;
            _domain = configuration.Domain;
            _incomingDomain = configuration.IncomingDomain;
            _requestAddress = configuration.RequestAddress;
        }

        public bool IsExpectedRecipient()
        {
            return _envelope.To.Count == 1
                && _envelope.To.Mailboxes.Any(e => e.Address == _requestAddress);
        }

        public AliasResponse NewResponseWith(Guid guid)
        {
            var message = new AliasResponse
            {
                Subject = $"A {_domain} alias has been created as requested.",
                Recipient = _envelope.From.FirstOrDefault() as MailboxAddress,
                Sender = _envelope.To.FirstOrDefault() as MailboxAddress,
                Content = $"{guid.ToString()}@{_incomingDomain}"
            };

            return message;
        }
    }
}