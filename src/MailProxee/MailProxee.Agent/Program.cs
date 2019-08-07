using CommandLine;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MailProxee.Agent
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureHostConfiguration(builder =>
                {
                    builder.SetBasePath(Directory.GetCurrentDirectory());
                    builder.AddJsonFile("hostsettings.json", optional: true);
                    builder.AddEnvironmentVariables(prefix: "MPX_");
                    builder.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    builder.SetBasePath(Directory.GetCurrentDirectory());
                    builder.AddJsonFile("appsettings.json", optional: true);
                    builder.AddJsonFile(
                        $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                        optional: true);
                    builder.AddEnvironmentVariables(prefix: "MPX_");
                    builder.AddCommandLine(args);
                })
                .ConfigureLogging((hostContext, configLog) =>
                {
                    var section = hostContext.Configuration.GetSection("Logging");
                    configLog.AddConfiguration(section);

                    if(hostContext.HostingEnvironment.IsDevelopment())
                    {
                        configLog.AddDebug();
                    }
                    configLog.AddConsole();
                })
                .ConfigureServices(services =>
                    services.AddHostedService<MailManagementService>())
                .UseConsoleLifetime()
                .Build();

            await host.RunAsync();
        }
    }
}
