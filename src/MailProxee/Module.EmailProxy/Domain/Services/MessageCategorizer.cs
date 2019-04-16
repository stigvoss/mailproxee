using System;
using System.Linq;
using System.Text.RegularExpressions;
using MailKit;
using MimeKit;
using Module.EmailProxy.Infrastructure;

namespace Module.EmailProxy.Domain.Services
{
    public enum MessageCategory
    {
        Unknown,
        Request,
        Delete,
        Incoming,
        Reply
    }

    public class MessageCategorizer
    {
        public MessageCategorizer(IInternetDomainConfiguration configuration)
        {
            _requestAddress = configuration.RequestAddress;
            _incomingDomain = configuration.IncomingDomain;
        }
        
        private readonly string _requestAddress;
        private readonly string _incomingDomain;

        public MessageCategory Categorize(Message message)
        {
            var category = MessageCategory.Unknown;

            if (IsRequest(message))
            {
                category = MessageCategory.Request;
            }
            else if (IsIncoming(message))
            {
                category = MessageCategory.Incoming;
            }

            return category;
        }

        private bool IsIncoming(Message message)
        {
            return message.Recipients.Any(address => IsIncomingAddress(address));
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

        private bool IsRequest(Message message)
        {
            return message.Recipients.Any(address => address == _requestAddress)
                && message.Recipients.Count() == 1;
        }
    }
}