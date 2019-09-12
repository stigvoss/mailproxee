using Module.EmailProxy.Domain.Base;
using Module.EmailProxy.Infrastructure;
using Module.EmailProxy.Infrastructure.Base;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Module.EmailProxy.Domain
{
    public class Mailman
    {
        private static readonly ConcurrentDictionary<Guid, string> _forwarding = new ConcurrentDictionary<Guid, string>();

        private readonly MailClient _client;
        private readonly IRepository<Alias> _aliases;
        private readonly IInternetDomainConfiguration _configuration;

        public Mailman(MailClient client, IRepository<Alias> aliases, IInternetDomainConfiguration configuration)
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
                var isActivated = alias?.ActivationCriteria?.IsActivated ?? false;

                if (alias is object && isActivated)
                {
                    var replyTo = $"{Guid.NewGuid().ToString()}@{_configuration.ReplyDomain}";

                    message.ConfigureForwarding(alias, replyTo);

                    await _client.Send(message)
                        .ConfigureAwait(false);
                }
            }
        }
    }
}
