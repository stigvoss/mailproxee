using System;
using System.Collections.Generic;
using System.Text;

namespace Module.EmailProxy.Infrastructure.Base
{
    public interface IDatabaseConfiguration
    {
        string ConnectionString { get; }
    }
}
