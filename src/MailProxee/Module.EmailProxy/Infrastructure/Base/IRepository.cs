using Module.EmailProxy.Domain.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Module.EmailProxy.Infrastructure.Base
{
    public interface IRepository<T>
        where T : IAggregateRoot
    {
        Task<IEnumerable<T>> All();

        Task<T> Find(Guid id);

        Task<T> Update(T item);

        Task<T> Add(T item);

        Task<T> Remove(T item);
    }
}
