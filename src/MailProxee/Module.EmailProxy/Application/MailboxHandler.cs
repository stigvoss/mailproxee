﻿using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using Module.EmailProxy.Domain;
using Module.EmailProxy.Domain.Base;
using Module.EmailProxy.Domain.Services;
using Module.EmailProxy.Infrastructure;
using Module.EmailProxy.Infrastructure.Base;
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
        private readonly Mailman _mailman;
        private readonly ILogger _logger;

        public MailboxHandler(
            IMailboxHandlerConfiguration mailboxHandler,
            IAliasRepository aliases,
            ILogger<MailboxHandler> logger = null)
        {
            _client = new MailClient(mailboxHandler, logger);
            _categorizer = new MessageCategorizer(mailboxHandler);
            _mailman = new Mailman(_client, aliases, mailboxHandler);
            _logger = logger;
        }

        public async Task HandleMessages(CancellationToken token)
        {
            await Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        _logger?.LogDebug("Fetching... ");
                        var messages = await _client.FetchMessages();
                        _logger?.LogInformation($"Fetched {messages.Count()} messages.");

                        try
                        {
                            foreach (var message in messages)
                            {
                                _logger?.LogDebug("Categorizing message... ");
                                try
                                {
                                    var category = _categorizer.Categorize(message);
                                    _logger?.LogInformation($"Categorized message as {category.ToString()}.");

                                    _logger?.LogDebug("Processing message... ");
                                    switch (category)
                                    {
                                        case MessageCategory.Incoming:
                                            await _mailman.ForwardEmail(message)
                                                .ConfigureAwait(false);
                                            break;
                                    }
                                    _logger?.LogDebug("Done.");

                                    _logger?.LogDebug("Mark for deletion... ");
                                    await _client.PermitDeletion(message);
                                    _logger?.LogInformation("Marked for deletion.");
                                }
                                catch (Exception ex)
                                {
                                    _logger?.LogError(ex, ex.Message);
                                }
                            }
                        }
                        finally
                        {
                            await _client.DisconnectSmtp();
                        }

                        _logger?.LogDebug("Deleting messages... ");
                        await _client.DeleteMessages()
                            .ConfigureAwait(false);
                        _logger?.LogInformation("Deleted messages.");
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, ex.Message);
                    }

                    try
                    {
                        _logger?.LogDebug("Entering sleep... ");
                        await Task.Delay(10000, token);
                        _logger?.LogDebug("Continuing.");
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