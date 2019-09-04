using Microsoft.EntityFrameworkCore;
using Module.EmailProxy.Domain;
using Module.EmailProxy.Infrastructure.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Module.EmailProxy.Infrastructure.EntityFrameworkCore.Repositories
{
    public class AliasRepository : IAliasRepository
    {
        private readonly AliasContext _context;

        public AliasRepository(AliasContext context)
        {
            _context = context;
        }

        public async Task<Alias> Add(Alias item)
        {
            var entry = await _context.Aliases.AddAsync(item)
                .ConfigureAwait(false);

            await _context.SaveChangesAsync()
                .ConfigureAwait(false);

            return entry.Entity;
        }

        public async Task<IQueryable<Alias>> All()
        {
            await Task.Yield();
            return _context.Aliases;
        }

        public async Task<Alias> Find(Guid id)
        {
            return await _context.Aliases.FindAsync(id);
        }

        public async Task<Alias> Remove(Alias item)
        {
            await Task.Yield();
            var entry = _context.Aliases.Remove(item);

            return entry.Entity;
        }

        public async Task<Alias> Update(Alias item)
        {
            await Task.Yield();
            var entry = _context.Aliases.Update(item);

            return entry.Entity;
        }
    }
}
