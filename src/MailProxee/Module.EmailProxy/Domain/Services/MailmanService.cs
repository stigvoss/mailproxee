using Module.EmailProxy.Domain.Base;
using Module.EmailProxy.Infrastructure;
using Module.EmailProxy.Infrastructure.Base;
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
        private readonly IRepository<Alias> _aliases;
        private readonly IInternetDomainConfiguration _configuration;

        public MailmanService(MailClient client, IRepository<Alias> aliases, IInternetDomainConfiguration configuration)
        {
            _client = client;
            _aliases = aliases;
            _configuration = configuration;
        }

        internal async Task ForwardEmail(Message message)
        {
            var identifiers = message.Recipients.ToList()
                   .Select(address => address.Split('@').FirstOrDefault())
                   .Where(identifier => identifier is object)
                   .Where(identifier => Guid.TryParse(identifier, out var _))
                   .Select(identifier => Guid.Parse(identifier));

            foreach (var identifier in identifiers)
            {
                var alias = await _aliases.Find(identifier);

                if (alias is object)
                {
                    var replyTo = $"{Guid.NewGuid().ToString()}@{_configuration.ReplyDomain}";

                    message.ConfigureForwarding(alias, replyTo);

                    await _client.Send(message)
                        .ConfigureAwait(false);
                }
            }
        }

        internal async Task SendNewAlias(Message message)
        {
            var requester = message.Senders.FirstOrDefault();

            var alias = new Alias(requester);
            await _aliases.Add(alias)
                .ConfigureAwait(false);

            var sender = message.Recipients.FirstOrDefault();

            var builder = new StringBuilder();

            builder.AppendLine($"Hello {alias.Recipient}!");
            builder.AppendLine();
            builder.AppendLine($"We are happy to announce that your alias is ready for use.");
            builder.AppendLine($"Any email sent to {alias.Id}@{_configuration.IncomingDomain} will be forwarded to {alias.Recipient}.");
            builder.AppendLine();
            builder.AppendLine($"It is currently not possible to respond to any received addresses using your {_configuration.Domain} alias.");
            builder.AppendLine();
            builder.AppendLine("Best regards,");
            builder.AppendLine($"{_configuration.Domain}");

            var subject = $"A {_configuration.Domain} alias was created.";

            await _client.Send(new Message(alias.Recipient, sender, subject, builder.ToString()))
                .ConfigureAwait(false);
        }
    }
}
