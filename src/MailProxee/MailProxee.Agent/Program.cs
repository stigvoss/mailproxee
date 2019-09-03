using CommandLine;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MimeKit;
using Module.EmailProxy.Infrastructure.EntityFrameworkCore;
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
                .ConfigureHostConfiguration(builder => ConfigureHost(builder, args))
                .ConfigureAppConfiguration((hostContext, builder) =>
                    ConfigureApp(hostContext, builder, args))
                .ConfigureLogging(ConfigureLogging)
                .ConfigureServices(ConfigureServices)
                .UseConsoleLifetime()
                .Build();

            await host.RunAsync();
        }

        private static void ConfigureHost(IConfigurationBuilder builder, string[] args)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("hostsettings.json", optional: true);
            builder.AddEnvironmentVariables(prefix: "MPX_");
            builder.AddCommandLine(args);
        }

        private static void ConfigureApp(HostBuilderContext context, IConfigurationBuilder builder, string[] args)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("appsettings.json", optional: true);
            builder.AddJsonFile(
                $"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
                optional: true);
            builder.AddEnvironmentVariables(prefix: "MPX_");
            builder.AddCommandLine(args);
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            var appSettings = new AppSettings();
            context.Configuration.Bind(appSettings);

            services.AddDbContext<AliasContext>(options =>
                options.UseNpgsql(appSettings.ConnectionStrings.Default));

            services.AddHostedService<MailManagementService>();
        }

        private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
        {
            var section = context.Configuration.GetSection("Logging");
            builder.AddConfiguration(section);

            if (context.HostingEnvironment.IsDevelopment())
            {
                builder.AddDebug();
            }
            builder.AddConsole();
        }
    }
}
