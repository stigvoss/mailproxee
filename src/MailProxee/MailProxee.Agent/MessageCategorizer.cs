using System;
using System.Linq;
using System.Text.RegularExpressions;
using MailKit;
using MimeKit;

namespace MailProxee.Agent
{
    public enum MessageCategory
    {
        Unknown,
        Request,
        Delete,
        Incoming,
        Reply
    }

    internal class MessageCategorizer
    {
        public MessageCategorizer(Configuration configuration)
        {
            _requestAddress = configuration.RequestAddress;
            _incomingDomain = configuration.IncomingDomain;
        }
        
        private readonly string _requestAddress;
        private readonly string _incomingDomain;

        internal MessageCategory Categorize(Envelope envelope)
        {
            var category = MessageCategory.Unknown;

            if (IsRequest(envelope))
            {
                category = MessageCategory.Request;
            }
            else if (IsIncoming(envelope))
            {
                category = MessageCategory.Incoming;
            }

            return category;
        }

        private bool IsIncoming(Envelope envelope)
        {
            return envelope.To.Mailboxes.Any(e => IsIncomingAddress(e.Address));
        }

        private bool IsIncomingAddress(string address)
        {
            var match = Regex.Match(address, "^(?<identifier>.+?)@(?<domain>.+?)$");

            if (!match.Success)
            {
                return false;
            }

            var identifier = match.Groups["identifier"]?.Value;
            var domain = match.Groups["domain"]?.Value;

            return Guid.TryParse(identifier, out var guid)
                && domain.ToLowerInvariant() == _incomingDomain;
        }

        private bool IsRequest(Envelope envelope)
        {
            return envelope.To.Mailboxes.Any(e => e.Address == _requestAddress)
                && envelope.To.Count == 1;
        }
    }
}