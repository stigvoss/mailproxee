using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using MailKit.Security;

namespace Module.EmailProxy.Infrastructure
{
    public interface IMailServerConfiguration
    {
        string Host { get; }

        int ImapPort { get; }

        int SmtpPort { get; }

        string UserName { get; }

        string Password { get; }
    }
}
