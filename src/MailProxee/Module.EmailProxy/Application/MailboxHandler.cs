using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Module.EmailProxy.Domain;
using Module.EmailProxy.Domain.Base;
using Module.EmailProxy.Domain.Repositories;
using Module.EmailProxy.Domain.Services;
using Module.EmailProxy.Infrastructure;
using Module.EmailProxy.Infrastructure.Base;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Module.EmailProxy.Application
{
    public class MailboxHandler : IDisposable
    {
        private readonly MailClient _client;
        private readonly MessageCategorizer _categorizer;
        private readonly IAliasRepository _aliases;
        private readonly MailmanService _mailman;

        public MailboxHandler(IMailboxHandlerConfiguration mailboxHandler, IDataSourceConfiguration dataSource)
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = dataSource.Host,
                Port = dataSource.Port,
                UserID = dataSource.UserName,
                Password = dataSource.Password,
                Database = dataSource.DataSource
            };
            var connection = new MySqlConnection(builder.ConnectionString);

            _client = new MailClient(mailboxHandler);
            _categorizer = new MessageCategorizer(mailboxHandler);
            _aliases = new AliasRepository(connection);
            _mailman = new MailmanService(_client, _aliases, mailboxHandler);
        }

        public async Task HandleMessages(CancellationToken token)
        {
            await _client.PrepareConnection();

            await Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        var messages = await _client.FetchMessages();

                        foreach (var message in messages)
                        {
                            try
                            {
                                var category = _categorizer.Categorize(message);

                                switch (category)
                                {
                                    case MessageCategory.Request:
                                        await _mailman.SendNewAlias(message)
                                            .ConfigureAwait(false);
                                        break;
                                    case MessageCategory.Incoming:
                                        await _mailman.ForwardEmail(message)
                                            .ConfigureAwait(false);
                                        break;
                                }

                                await _client.PermitDeletion(message);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex);
                            }
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