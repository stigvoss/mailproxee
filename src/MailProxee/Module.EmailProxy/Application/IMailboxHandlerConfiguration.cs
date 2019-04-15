using Module.EmailProxy.Infrastructure;
using Module.EmailProxy.Infrastructure.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace Module.EmailProxy.Application
{
    public interface IMailboxHandlerConfiguration
        : IInternetDomainConfiguration, IMailClientConfiguration
    { }
}
