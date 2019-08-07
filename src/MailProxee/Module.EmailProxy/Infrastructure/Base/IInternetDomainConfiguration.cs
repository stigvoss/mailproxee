using System;
using System.Collections.Generic;
using System.Text;

namespace Module.EmailProxy.Infrastructure.Base
{
    public interface IInternetDomainConfiguration
    {
        string RequestAddress { get; }

        string IncomingDomain { get; }

        string ReplyDomain { get; }

        string Domain { get; }
    }
}
