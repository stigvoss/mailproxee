using Module.EmailProxy.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Module.EmailProxy.Domain.Services
{
    public class MailmanService
    {
        private static readonly ConcurrentDictionary<Guid, string> _forwarding = new ConcurrentDictionary<Guid, string>();

        private readonly MailClient _client;
        private readonly IInternetDomainConfiguration _configuration;

        public MailmanService(MailClient client, IInternetDomainConfiguration configuration)
        {
            _client = client;
            _configuration = configuration;
        }

        internal async Task ForwardEmail(Message message)
        {
            var recipients = message.Recipients
                   .Select(address => address.Split('@').FirstOrDefault())
                   .Where(identifier => identifier is object)
                   .Where(identifier => Guid.TryParse(identifier, out var _))
                   .Select(identifier => Guid.Parse(identifier))
                   .Where(guid => _forwarding.ContainsKey(guid))
                   .Select(guid => _forwarding[guid]);

            var alias = Guid.NewGuid().ToString();
            var sender = $"{alias}@{_configuration.ReplyDomain}";

            message.Senders = new[] { sender };
            message.Recipients = recipients;

            await _client.Send(message);
        }

        internal async Task SendNewAlias(Message message)
        {
            var alias = Guid.NewGuid().ToString();

            var recipient = message.Senders.FirstOrDefault();
            var sender = message.Recipients.FirstOrDefault();

            var subject = $"A {_configuration.Domain} alias has been created as requested.";
            var body = $"{alias}@{_configuration.IncomingDomain}";

            await _client.Send(new Message(recipient, sender, subject, body))
                .ConfigureAwait(false);
        }
    }
}
