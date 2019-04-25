using Module.EmailProxy.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Module.EmailProxy.Infrastructure.Base
{
    public interface IAliasRepository
        : IRepository<Alias>
    {
    }
}
