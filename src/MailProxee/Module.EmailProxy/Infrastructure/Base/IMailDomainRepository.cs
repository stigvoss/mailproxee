using Module.EmailProxy.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Module.EmailProxy.Infrastructure.Base
{
    public interface IMailDomainRepository
        : IRepository<MailDomain>
    {
        Task<MailDomain> Find(string domainName);
    }
}
