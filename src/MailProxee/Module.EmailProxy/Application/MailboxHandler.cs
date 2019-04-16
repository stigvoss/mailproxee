using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Module.EmailProxy.Domain;
using Module.EmailProxy.Domain.Services;
using Module.EmailProxy.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Module.EmailProxy.Application
{
    public class MailboxHandler : IDisposable
    {
        private readonly IMailboxHandlerConfiguration _configuration;
        private readonly MailClient _client;

        public MailboxHandler(IMailboxHandlerConfiguration configuration)
        {
            _configuration = configuration;
            _client = new MailClient(configuration);
        }

        public async Task HandleMessages(CancellationToken token)
        {
            await _client.PrepareConnection();

            await Task.Run(async () =>
                {
                    var categorizer = new MessageCategorizer(_configuration);
                    var mailman = new MailmanService(_client, _configuration);

                    while (!token.IsCancellationRequested)
                    {
                        var messages = await _client.FetchMessages();

                        foreach (var message in messages)
                        {
                            var category = categorizer.Categorize(message);

                            switch (category)
                            {
                                case MessageCategory.Request:
                                    await mailman.SendNewAlias(message)
                                        .ConfigureAwait(false);
                                    break;
                                case MessageCategory.Incoming:
                                    await mailman.ForwardEmail(message)
                                        .ConfigureAwait(false);
                                    break;
                            }

                            await _client.PermitDeletion(message);
                        }

                        await _client.DeleteMessages()
                            .ConfigureAwait(false);

                        try
                        {
                            await Task.Delay(10000, token);
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                        }
                    }
                }).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}