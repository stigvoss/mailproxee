﻿using CommandLine;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Module.EmailProxy.Application;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MailProxee.Agent
{
    class Program
    {
        private static readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        static void Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<Options>(args)
                .WithParsed(async options => await Run(options));
        }

        private static async Task Run(Options options)
        {
            var configuration = await Configuration.LoadFrom(options.Configuration);

            using (var handler = new MailboxHandler(configuration, configuration.Database))
            {
                var messageHandler = handler.HandleMessages(_tokenSource.Token)
                    .ConfigureAwait(false);

                Console.WriteLine("Press [Enter] to exit...");
                Console.ReadLine();

                Console.WriteLine("Stopping...");
                _tokenSource.Cancel();

                await messageHandler;
            }
        }
    }

    public class Options
    {
        [Option('c', "config", Required = true)]
        public string Configuration { get; set; }
    }
}
