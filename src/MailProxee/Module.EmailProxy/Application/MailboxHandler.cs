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
using Module.EmailProxy.Infrastructure.Repositories;
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
            var aliases = new AliasRepository(connection);
            _mailman = new MailmanService(_client, aliases, mailboxHandler);
        }

        public async Task HandleMessages(CancellationToken token)
        {
            Console.Write("Connecting... ");
            await _client.PrepareMailboxConnection();
            Console.WriteLine("Done.");

            await Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    Console.Write("Fetching... ");
                    var messages = await _client.FetchMessages();
                    Console.WriteLine($"{messages.Count()} messages.");

                    try
                    {
                        foreach (var message in messages)
                        {
                            Console.Write("Categorizing message... ");
                            try
                            {
                                var category = _categorizer.Categorize(message);
                                Console.WriteLine($"{category.ToString()}.");

                                Console.Write("Processing message... ");
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
                                Console.WriteLine("Done.");

                                Console.Write("Mark for deletion... ");
                                await _client.PermitDeletion(message);
                                Console.WriteLine("Done.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }
                    }
                    finally
                    {
                        await _client.DisconnectSmtp();
                    }

                    Console.Write("Deleting messages... ");
                    await _client.DeleteMessages()
                        .ConfigureAwait(false);
                    Console.WriteLine("Done.");

                    try
                    {
                        Console.Write("Entering sleep... ");
                        await Task.Delay(10000, token);
                        Console.WriteLine("Continuing.");
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