using Module.EmailProxy.Domain;
using Module.EmailProxy.Infrastructure.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Module.EmailProxy.Infrastructure.EntityFrameworkCore.Repositories
{
    public class MailDomainRepository
        : IMailDomainRepository
    {
        private readonly AliasContext _context;

        public MailDomainRepository(AliasContext context)
        {
            _context = context;
        }

        public async Task<MailDomain> Add(MailDomain item)
        {
            var entry = await _context.Domains.AddAsync(item)
                .ConfigureAwait(false);

            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return entry.Entity;
        }

        public async Task<IQueryable<MailDomain>> All()
        {
            await Task.Yield();
            return _context.Domains;
        }

        public async Task<MailDomain> Find(Guid id)
        {
            return await _context.Domains.FindAsync(id);
        }

        public async Task<MailDomain> Find(string domainName)
        {
            await Task.Yield();
            return _context.Domains.FirstOrDefault(e => e.Name == domainName);
        }

        public async Task<MailDomain> Remove(MailDomain item)
        {
            var entry = _context.Domains.Remove(item);

            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return entry.Entity;
        }

        public async Task<MailDomain> Update(MailDomain item)
        {
            await Task.Yield();
            var entry = _context.Domains.Update(item);

            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return entry.Entity;
        }
    }
}
