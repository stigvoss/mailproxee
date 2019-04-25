using Module.EmailProxy.Domain.Base;
using Module.EmailProxy.Infrastructure.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Module.EmailProxy.Domain.Repositories
{
    public class InMemoryAliasRepository : IAliasRepository
    {
        private List<Alias> _aliases = new List<Alias>();

        public async Task<Alias> Add(Alias item)
        {
            await Task.Delay(200);

            if (!_aliases.Any(e => e == item))
            {
                _aliases.Add(item);
                return item;
            }

            return null;
        }

        public async Task<IEnumerable<Alias>> All()
        {
            await Task.Delay(200);

            return _aliases;
        }

        public async Task<Alias> Find(Guid id)
        {
            await Task.Delay(200);

            return _aliases.FirstOrDefault(e => e.Id == id);
        }

        public async Task<Alias> Remove(Alias item)
        {
            await Task.Delay(200);

            if (_aliases.Remove(item))
            {
                return item;
            }

            return null;
        }

        public async Task<Alias> Update(Alias item)
        {
            await Task.Delay(200);

            int index;

            if ((index = _aliases.IndexOf(item)) != -1)
            {
                _aliases.RemoveAt(index);
                _aliases.Insert(index, item);

                return item;
            }

            return null;
        }
    }
}
