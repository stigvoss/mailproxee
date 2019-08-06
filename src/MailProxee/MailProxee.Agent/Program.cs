using CommandLine;
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

        static async Task Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<Options>(args);

            if (result is Parsed<Options> options)
            {
                await Run(options.Value);
            }
        }

        private static async Task Run(Options options)
        {
            try
            {
                var configuration = await Configuration.LoadFrom(options.Configuration);

                using (var handler = new MailboxHandler(configuration.Mailbox, configuration.Database))
                {
                    Console.WriteLine("Press [Enter] to exit...");

                    var messageHandler = handler.HandleMessages(_tokenSource.Token);

                    Console.ReadLine();

                    Console.WriteLine("Stopping...");
                    _tokenSource.Cancel();

                    await messageHandler;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine(ex);
            }
        }
    }

    public class Options
    {
        [Option('c', "config", Required = true)]
        public string Configuration { get; set; }
    }
}
