using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using MailKit.Security;

namespace Module.EmailProxy.Infrastructure.Base
{
    public interface IMailClientConfiguration
    {
        string Host { get; }

        int ImapPort { get; }

        int SmtpPort { get; }

        string UserName { get; }

        string Password { get; }
    }
}
