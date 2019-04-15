using System;
using System.Collections.Generic;
using System.Text;

namespace Module.EmailProxy.Infrastructure.Base
{
    public interface IDataSourceConfiguration
    {
        string Host { get; }

        ushort Port { get; }

        string UserName { get; }

        string Password { get; }

        string DataSource { get; }
    }
}
