﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Module.EmailProxy.Application;
using Module.EmailProxy.Infrastructure.EntityFrameworkCore;
using Module.EmailProxy.Infrastructure.EntityFrameworkCore.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MailProxee.Agent
{
    public class MailManagementService : IHostedService, IDisposable
    {
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        private readonly MailboxHandler _handler;
        private Task _messageHandler;

        public MailManagementService(IConfiguration configuration, AliasContext context, ILogger<MailboxHandler> logger)
        {
            var appSettings = new AppSettings();
            configuration.Bind(appSettings);

            var aliases = new AliasRepository(context);

            _handler = new MailboxHandler(appSettings.Mailbox, aliases, logger);
        }

        public IConfiguration Configuration { get; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _messageHandler = _handler.HandleMessages(_tokenSource.Token);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _tokenSource.Cancel();

            await _messageHandler;
        }

        public void Dispose()
        {
            _handler.Dispose();
            _tokenSource.Dispose();
        }
    }
}
